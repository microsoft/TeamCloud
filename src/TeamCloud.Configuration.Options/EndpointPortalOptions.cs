/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Configuration.Options
{
    [Options("Endpoint:Portal")]
    public sealed class EndpointPortalOptions
    {
        public string Url { get; set; }

        public string AuthCode { get; set; }
    }
}
