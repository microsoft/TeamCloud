/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Linq;
using TeamCloud.Adapters.Authorization;

namespace TeamCloud.Adapters
{
    public abstract class Adapter : IAdapter
    {
        private readonly IAuthorizationSessionClient sessionClient;
        private readonly IAuthorizationTokenClient tokenClient;

        protected Adapter(IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient)
        {
            this.sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
            this.tokenClient = tokenClient ?? throw new ArgumentNullException(nameof(tokenClient));
        }

        protected IAuthorizationSessionClient SessionClient
            => sessionClient;

        protected IAuthorizationTokenClient TokenClient
            => tokenClient;

        protected string GetHtml(string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                throw new ArgumentException($"'{nameof(suffix)}' cannot be null or whitespace.", nameof(suffix));

            var resourceName = GetType().Assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.Equals($"{GetType().FullName}_{suffix.Trim()}.html", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(resourceName))
            {
                using var stream = GetType().Assembly.GetManifestResourceStream($"{this.GetType().FullName}.html");
                using var streamReader = new StreamReader(stream);

                var html = streamReader.ReadToEnd();



                return html;
            }

            return null;
        }
    }
}
