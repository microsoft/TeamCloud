/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Exceptions
{
    public class ProviderException : Exception
    {
        public string ProviderId { get; private set; }

        public ProviderException(Provider provider, Exception exception)
            : base($"An exception was trown by Provider: {provider.Id}", exception)
        {
            ProviderId = provider.Id;
        }
    }
}
