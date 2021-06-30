/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options
{
    [Options]
    public sealed class TeamCloudAdapterAuthorizeOptions : IAuthorizationSessionOptions, IAuthorizationTokenOptions
    {
        private readonly AdapterSessionStorageOptions sessionStorageOptions;
        private readonly AdapterTokenStorageOptions tokenStorageOptions;

        public TeamCloudAdapterAuthorizeOptions(AdapterSessionStorageOptions sessionStorageOptions, AdapterTokenStorageOptions tokenStorageOptions)
        {
            this.sessionStorageOptions = sessionStorageOptions ?? throw new ArgumentNullException(nameof(sessionStorageOptions));
            this.tokenStorageOptions = tokenStorageOptions ?? throw new ArgumentNullException(nameof(tokenStorageOptions));
        }

        string IAuthorizationSessionOptions.ConnectionString => sessionStorageOptions.ConnectionString;

        string IAuthorizationTokenOptions.ConnectionString => tokenStorageOptions.ConnectionString;
    }
}
