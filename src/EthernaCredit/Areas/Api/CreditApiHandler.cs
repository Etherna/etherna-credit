// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Authentication;
using Etherna.BeeNet.Models;
using Etherna.Credit.Areas.Api.DtoModels;
using Etherna.Credit.Areas.Api.InputModels;
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Events;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.Credit.Services.Domain;
using Etherna.Credit.Shkeeper;
using Etherna.Credit.Shkeeper.Models;
using Etherna.DomainEvents;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Api
{
    internal sealed class CreditApiHandler(
        ICreditDbContext dbContext,
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IEventDispatcher eventDispatcher,
        IShKeeperService shkeperService,
        IUserService userService)
        : ICreditApiHandler
    {
        // Consts.
        /// <summary>
        /// Large static invoice amount that keeps the SHKeeper invoice permanently in PARTIAL status,
        /// enabling the static address mode: one reusable deposit address per user per crypto.
        /// </summary>
        private static readonly XDaiValue StaticInvoiceAmount = 1_000_000_000M;

        // Methods.
        public Task<IResult> CallbackCryptoPaymentAsync(string apiKey, CallbackPaymentRequestInput body, string secret) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Compare Api key.
                var validApiKey = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(shkeperService.ApiKey),
                    Encoding.UTF8.GetBytes(apiKey)
                );
                
                // Get wallet record and verify confirm secret.
                // ExternalId format: "{authorId}.{cryptoSymbol}" (see GetCryptoWalletAsync).
                // ObjectIds are 24-char hex with no dots; crypto symbols use hyphens, never dots.
                var dotIdx = body.ExternalId.IndexOf('.', StringComparison.InvariantCulture);
                var authorId = dotIdx >= 0 ? body.ExternalId[..dotIdx] : string.Empty;
                var symbol   = dotIdx >= 0 ? body.ExternalId[(dotIdx + 1)..] : string.Empty;
                var userWallet = await dbContext.UserCryptoWallets.TryFindOneAsync(
                    w => w.Author.Id == authorId && w.Symbol == symbol);

                // Use a dummy value when the wallet is not found to preserve constant-time comparison.
                var storedSecret = userWallet?.ConfirmSecret ?? Guid.NewGuid().ToString();
                var validSecret = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(storedSecret),
                    Encoding.UTF8.GetBytes(secret)
                );

                if (!validApiKey || !validSecret)
                    return Results.NotFound();

                // Process each transaction that triggered this callback.
                // Multiple triggers can occur in a single callback.
                foreach (var triggerTx in body.Transactions.Where(t => t.Trigger))
                {
                    // Determine the credited amount net of SHKeeper fees.
                    // Can be negative when fees exceed the transaction amount: skip in that case.
                    var creditedAmount = decimal.Parse(triggerTx.AmountFiatWithoutFee, CultureInfo.InvariantCulture);
                    if (creditedAmount <= 0)
                        continue;

                    // Idempotency guard: skip if this transaction was already processed.
                    var alreadyProcessed = await dbContext.ProcessedCryptoTransactions.TryFindOneAsync(
                        t => t.TxId == triggerTx.TxId);
                    if (alreadyProcessed is not null)
                        continue;

                    // Record the transaction as processed.
                    // The unique index on TxId acts as a safety net for rare concurrent duplicates.
                    var processedTx = new ProcessedCryptoTransaction(triggerTx.TxId, userWallet!);
                    await dbContext.ProcessedCryptoTransactions.CreateAsync(processedTx);

                    // Credit user balance.
                    await userService.TryIncrementUserBalanceAsync(userWallet!.Author, creditedAmount, false);

                    // Report log.
                    var depositLog = new CryptoDepositOperationLog(
                        creditedAmount,
                        triggerTx.AmountCrypto,
                        triggerTx.Crypto,
                        "shkeeper",
                        userWallet.Author);
                    await dbContext.OperationLogs.CreateAsync(depositLog);

                    // Dispatch event.
                    await eventDispatcher.DispatchAsync(new UserDepositEvent(depositLog));
                }

                return Results.Accepted();
            });

        public Task<IResult> GetAvailablePaymentCryptosAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                var cryptos = await shkeperService.GetAvailableCryptosAsync();
                return Results.Json(cryptos.Values.Select(c => new PaymentCryptoDto(c.DisplayName, c.Symbol)));
            });

        public Task<IResult> GetCryptoWalletAsync(string cryptoSymbol, HttpRequest request) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Verify auth and input.
                var (author, _) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
                var availableCryptos = await shkeperService.GetAvailableCryptosAsync();
                if (!availableCryptos.ContainsKey(cryptoSymbol))
                    return Results.BadRequest($"Crypto symbol {cryptoSymbol} not found");

                // Find or create the static wallet record for this user/symbol combination.
                // One persistent wallet per user per crypto is reused indefinitely (static address mode).
                var externalId = $"{author.Id}.{cryptoSymbol}";
                var userWallet = await dbContext.UserCryptoWallets.TryFindOneAsync(
                    w => w.Author.Id == author.Id && w.Symbol == cryptoSymbol);

                ShKeeperPaymentRequestResponse shkeeperResponse;
                var callbackBaseUrl = shkeperService.CustomCallbackBaseUrl ?? $"{request.Scheme}://{request.Host}";
                if (userWallet is null)
                {
                    var confirmSecret = Guid.NewGuid().ToString().Replace("-", "", StringComparison.InvariantCulture);
                    shkeeperResponse = await shkeperService.CreateInvoiceAsync(
                        StaticInvoiceAmount,
                        $"{callbackBaseUrl}/api/v0.3/payments/crypto/internal/callback/{confirmSecret}",
                        cryptoSymbol,
                        externalId);
                    userWallet = new UserCryptoWallet(author, confirmSecret, cryptoSymbol, shkeeperResponse.Wallet);
                    await dbContext.UserCryptoWallets.CreateAsync(userWallet);
                }
                else
                {
                    // Refresh the invoice on SHKeeper (idempotent: same external_id reuses the same address).
                    shkeeperResponse = await shkeperService.CreateInvoiceAsync(
                        StaticInvoiceAmount,
                        $"{callbackBaseUrl}/api/v0.3/payments/crypto/internal/callback/{userWallet.ConfirmSecret}",
                        cryptoSymbol,
                        externalId);
                }

                // Get transactions registered on this address.
                var transactions = await shkeperService.GetInvoiceTxsAsync(externalId);

                return Results.Json(new CryptoWalletDto(
                    Wallet: userWallet.Wallet,
                    CoinSymbol: cryptoSymbol,
                    CoinDisplayName: shkeeperResponse.DisplayName,
                    ExchangeRate: shkeeperResponse.ExchangeRate,
                    RecalculateAfter: shkeeperResponse.RecalculateAfter,
                    Txs: transactions.Select(tx => new CryptoTxDto(
                        TxId: tx.TxId,
                        Address: tx.Address,
                        Amount: tx.Amount,
                        Crypto: tx.Crypto,
                        IsConfirmed: tx.IsConfirmed))));
            });

        public Task<IResult> GetCurrentUserAddressAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                EthAddress address = await ethernaOidcClient.GetEtherAddressAsync();
                return Results.Json(address);
            });

        public Task<IResult> GetCurrentUserCreditAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                var (userModel, _) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
                var balance = await userService.GetUserBalanceAsync(userModel);
                return Results.Json(new CreditDto(balance, userModel.HasUnlimitedCredit));
            });

        public Task<IResult> GetCurrentUserLogsAsync(int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var (userModel, userSharedInfo) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
                var paginatedLogs = await dbContext.OperationLogs.QueryPaginatedElementsAsync(
                    elements => elements.Where(l => l.User.Id == userModel.Id),
                    l => l.CreationDateTime,
                    page,
                    take);
                return Results.Json(paginatedLogs.Elements.Select(l => new OperationLogDto(l, userSharedInfo)));
            });
        
        public Task<IResult> GetServiceOpLogsWithUserAsync(
            EthAddress address,
            DateTime? fromDate,
            DateTime? toDate) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var clientId = await ethernaOidcClient.GetClientIdAsync();

                var (user, userSharedInfo) = await userService.TryFindUserAsync(address);
                if (user is null)
                    return Results.Json(Array.Empty<OperationLogDto>());

                var result = await dbContext.OperationLogs.QueryElementsAsync(
                    elements => elements.Where(l => l.Author == clientId)
                        .Where(l => l.User.Id == user.Id)
                        .Where(l => l.CreationDateTime >= (fromDate ?? DateTime.MinValue))
                        .Where(l => l.CreationDateTime <= (toDate ?? DateTime.MaxValue))
                        .OrderBy(l => l.CreationDateTime)
                        .ToListAsync());

                return Results.Json(result.Select(l => new OperationLogDto(l, userSharedInfo!)));
            });

        public Task<IResult> GetUserCreditAsync(EthAddress address) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var (user, _) = await userService.TryFindUserAsync(address);
                if (user is null)
                    return Results.Json(new CreditDto(0, false));

                var balance = await userService.GetUserBalanceAsync(user);

                return Results.Json(new CreditDto(balance, user.HasUnlimitedCredit));
            });

        public Task<IResult> RegisterBalanceUpdateAsync(
            EthAddress address,
            XDaiValue amount,
            bool isApplied,
            string reason) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var clientId = await ethernaOidcClient.GetClientIdAsync();

                // Get user.
                var (user, _) = await userService.FindUserAsync(address);

                // Apply update (balance can go negative).
                if (isApplied)
                {
                    var result = await userService.TryIncrementUserBalanceAsync(user, amount, true);
                    if (!result)
                        throw new InvalidOperationException();
                }

                // Create or update log.
                var updatedLog = await dbContext.OperationLogs.AccessToCollectionAsync(collection =>
                    collection.FindOneAndUpdateAsync(
                        Builders<OperationLogBase>.Filter.OfType<UpdateOperationLog>(
                            log => log.Author == clientId &&
                                   log.CreationDateTime >= DateTime.Now.Date &&
                                   log.IsApplied == isApplied &&
                                   log.Reason == reason &&
                                   log.User.Id == user.Id),
                        Builders<OperationLogBase>.Update.Inc(log => log.Amount, amount)));

                if (updatedLog is null) //if a previous log didn't exist
                {
                    var withdrawLog = new UpdateOperationLog(amount, clientId, isApplied, reason, user);
                    await dbContext.OperationLogs.CreateAsync(withdrawLog);
                }

                return Results.Ok();
            });
    }
}