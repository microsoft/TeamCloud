/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Services
{
    public interface IOrchestratorOptions
    {
        public string Url { get; }

        public string AuthCode { get; }
    }
}
