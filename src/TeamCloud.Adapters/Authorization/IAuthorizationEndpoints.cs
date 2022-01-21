/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Adapters.Authorization;

public interface IAuthorizationEndpoints
{
    string AuthorizationUrl { get; }

    string CallbackUrl { get; }
}
