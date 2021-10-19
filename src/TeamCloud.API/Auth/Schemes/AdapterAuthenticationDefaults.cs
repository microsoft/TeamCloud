using Microsoft.AspNetCore.Authentication.Cookies;

namespace TeamCloud.API.Auth.Schemes
{
    public static class AdapterAuthenticationDefaults
    {
        public const string AuthenticationScheme = "Adapter";

        public const string AuthenticationType = "Adapter";

        public const string QueryParam = "ott";

        public const string ClaimType = "ott";

        public static readonly string CookiePrefix = CookieAuthenticationDefaults.CookiePrefix;
    }
}
