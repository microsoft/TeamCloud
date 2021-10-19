using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;

namespace TeamCloud.API.Auth.Schemes
{
    public static class AdapterAuthenticationExtensions
    {
        public static AuthenticationBuilder AddAdapterAuthentication(this AuthenticationBuilder authenticationBuilder)
        {
            authenticationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureCookieAuthenticationOptions>());

            return authenticationBuilder.AddScheme<CookieAuthenticationOptions, AdapterAuthenticationHandler>(AdapterAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = true;
            });
        }
    }
}
