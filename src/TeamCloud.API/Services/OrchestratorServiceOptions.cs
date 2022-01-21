/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Services;

public interface IOrchestratorServiceOptions
{
    public string Url { get; }

    public string AuthCode { get; }
}
