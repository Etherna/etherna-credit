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
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Services.Views.Emails;
using Etherna.DomainEvents;
using Etherna.ServicesClient.Internal;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.EventHandlers
{
    internal sealed class OnUserDepositThenSendEmailHandler : EventHandlerBase<UserDepositEvent>
    {
        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IEthernaInternalSsoClient serviceSsoClient;
        private readonly ISharedDbContext sharedDbContext;

        // Constructor.
        public OnUserDepositThenSendEmailHandler(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            IEthernaInternalSsoClient serviceSsoClient,
            ISharedDbContext sharedDbContext)
        {
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.serviceSsoClient = serviceSsoClient;
            this.sharedDbContext = sharedDbContext;
        }

        // Methods.
        public override async Task HandleAsync(UserDepositEvent @event)
        {
            // Get user shared info.
            var userSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(
                @event.OperationLog.User.SharedInfoId);

            // Get user email.
            var contacts = await serviceSsoClient.ServiceInteract.ContactsAsync(userSharedInfo.EtherAddress);
            if (contacts.Email is null)
                return;

            // Generate email.
            var emailBody = await razorViewRenderer.RenderViewToStringAsync(
                "Views/Emails/DepositConfirmation.cshtml",
                new DepositConfirmationModel());

            // Send email.
            await emailSender.SendEmailAsync(
                contacts.Email,
                DepositConfirmationModel.Title,
                emailBody);
        }
    }
}
