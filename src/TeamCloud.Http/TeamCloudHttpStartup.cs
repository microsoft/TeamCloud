using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using TeamCloud.Http;

[assembly: FunctionsStartup(typeof(TeamCloudHttpStartup))]

namespace TeamCloud.Http
{
    public class TeamCloudHttpStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            DisableCertificateValidation();
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Security", "CA5359:Do Not Disable Certificate Validation", Justification = "Only for DEBUG builds")]
        private static void DisableCertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }
}
