/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Services
{
    public interface IOrchestratorOptions
    {
        public string Url { get; set; }

        public string AuthCode { get; set; }
    }
}
