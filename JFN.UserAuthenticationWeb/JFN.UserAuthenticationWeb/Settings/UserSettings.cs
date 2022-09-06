#nullable enable
namespace JFN.UserAuthenticationWeb.Settings
{
    public class UserSettings
    {
        public string Salt { get; set; } = "";
        public string? OverrideReturnUrl { get; set; }
    }
}
