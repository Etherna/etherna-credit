using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Services.Views.Emails;
using Etherna.DomainEvents;
using Etherna.ServicesClient.Clients.Sso;
using Etherna.SSL.Services;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.EventHandlers
{
    class OnUserWithdrawThenSendEmailHandler : EventHandlerBase<UserWithdrawEvent>
    {
        // Fields.
        private readonly IEmailSender emailSender;
        private readonly IRazorViewRenderer razorViewRenderer;
        private readonly IServiceSsoClient serviceSsoClient;
        private readonly ISharedDbContext sharedDbContext;

        // Constructor.
        public OnUserWithdrawThenSendEmailHandler(
            IEmailSender emailSender,
            IRazorViewRenderer razorViewRenderer,
            IServiceSsoClient serviceSsoClient,
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
            var userSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(@event.User.SharedInfoId);

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
