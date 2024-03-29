﻿//   Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.ACR.Services;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Services.Views.Emails;
using Etherna.DomainEvents;
using Etherna.ServicesClient.Internal;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.EventHandlers
{
    internal sealed class OnUserWithdrawThenSendEmailHandler : EventHandlerBase<UserWithdrawEvent>
    {
        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IEthernaInternalSsoClient serviceSsoClient;
        private readonly ISharedDbContext sharedDbContext;

        // Constructor.
        public OnUserWithdrawThenSendEmailHandler(
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
        public override async Task HandleAsync(UserWithdrawEvent @event)
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
