/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Diagnostics.CodeAnalysis;

namespace TeamCloud.Model
{
    public static class Constants
    {
        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Provides constants")]
        public static class CosmosDb
        {
            public const string DatabaseName = "TeamCloud";

            public const string TenantName = "TeamCloud";
        }
    }
}
