/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using TeamCloud.Http;
using TeamCloud.Http.Telemetry;

[assembly: WebJobsStartup(typeof(TeamCloudHttpStartup))]

namespace TeamCloud.Http
{
    public class TeamCloudHttpStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            DisableCertificateValidation();

            builder.AddExtension<TeamCloudTelemetryExtension>();
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Security", "CA5359:Do Not Disable Certificate Validation", Justification = "Only for DEBUG builds")]
        private static void DisableCertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }
}
