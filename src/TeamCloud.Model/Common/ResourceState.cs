﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResourceState
    {
        Pending,

        Initializing,

        Provisioning,

        Provisioned,

        Deprovisioning,

        Deprovisioned,

        Failed
    }

    public static class ResourceStateExtensions
    {
        public static bool IsFinal(this ResourceState resourceState)
            => resourceState == ResourceState.Provisioned || resourceState == ResourceState.Deprovisioned || resourceState == ResourceState.Failed;

        public static bool IsActive(this ResourceState resourceState)
            => !resourceState.IsFinal();
    }
}
