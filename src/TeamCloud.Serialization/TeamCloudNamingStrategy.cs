/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization;

public sealed class TeamCloudNamingStrategy : CamelCaseNamingStrategy
{
    public static TeamCloudNamingStrategy Default = new();

    public TeamCloudNamingStrategy()
        : base(processDictionaryKeys: false, overrideSpecifiedNames: true)
    { }
}
