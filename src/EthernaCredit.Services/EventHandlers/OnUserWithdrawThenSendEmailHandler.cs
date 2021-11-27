using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Services.Views.Emails;
using Etherna.DomainEvents;
using Etherna.RCL.Services;
using Etherna.ServicesClient.Clients.Sso;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.EventHandlers
{
    class OnUserWithdrawThenSendEmailHandler : EventHandlerBase<UserWithdrawEvent>
    {
        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IServiceSsoClient serviceSsoClient;

        // Constructor.
        public OnUserWithdrawThenSendEmailHandler(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            IServiceSsoClient serviceSsoClient)
        {
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.serviceSsoClient = serviceSsoClient;
        }

        // Methods.
        public override async Task HandleAsync(UserWithdrawEvent @event)
        {
            // Get user email.
            var contacts = await serviceSsoClient.ServiceInteract.ContactsAsync(@event.User.EtherAddress);
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
