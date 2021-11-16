namespace Etherna.CreditSystem.Services.Settings
{
    public class SsoServerSettings
    {
        public string BaseUrl { get; set; } = default!;
        public string LoginPath { get; set; } = default!;
        public string LoginUrl => BaseUrl + LoginPath;
        public string RegisterPath { get; set; } = default!;
        public string RegisterUrl => BaseUrl + RegisterPath;
    }
}
