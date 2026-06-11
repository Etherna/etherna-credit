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

using Etherna.ACR.Services;
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Events;
using Etherna.Credit.Services.Views.Emails;
using Etherna.DomainEvents;
using Etherna.Sdk.Internal.Clients;
using System.Threading.Tasks;

namespace Etherna.Credit.Services.EventHandlers
{
    internal sealed class OnUserWithdrawThenSendEmailHandler(
        IEmailSender emailSender,
        IRazorViewRenderer razorViewRenderer,
        IEthernaInternalSsoClient serviceSsoClient,
        ISharedDbContext sharedDbContext)
        : EventHandlerBase<UserWithdrawEvent>
    {
        // Methods.
        public override async Task HandleAsync(UserWithdrawEvent @event)
        {
            // Get user shared info.
            var userSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(
                @event.OperationLog.User.SharedInfoId);

            // Get user email.
            var contacts = await serviceSsoClient.GetUserContactsAsync(userSharedInfo.EtherAddress);
            if (contacts.Email is null)
                return;

            // Generate email.
            var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                "Views/Emails/WithdrawalConfirmation.cshtml",
                new WithdrawalConfirmationModel());

            // Send email.
            await emailSender.SendEmailAsync(
                contacts.Email,
                WithdrawalConfirmationModel.Title,
                emailBody);
        }
    }
}
