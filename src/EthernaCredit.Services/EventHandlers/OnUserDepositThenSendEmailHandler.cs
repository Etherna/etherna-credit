using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Services.Views.Emails;
using Etherna.DomainEvents;
using Etherna.ServicesClient.Clients.Sso;
using Etherna.SSL.Services;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.EventHandlers
{
    class OnUserDepositThenSendEmailHandler : EventHandlerBase<UserDepositEvent>
    {
        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IServiceSsoClient serviceSsoClient;

        // Constructor.
        public OnUserDepositThenSendEmailHandler(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            IServiceSsoClient serviceSsoClient)
        {
            this.emailSender = emailSender;
            this.razorViewRenderer = razorViewRenderer;
            this.serviceSsoClient = serviceSsoClient;
        }

        // Methods.
        public override async Task HandleAsync(UserDepositEvent @event)
        {
            // Get user email.
            var contacts = await serviceSsoClient.ServiceInteract.ContactsAsync(@event.User.EtherAddress);
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
