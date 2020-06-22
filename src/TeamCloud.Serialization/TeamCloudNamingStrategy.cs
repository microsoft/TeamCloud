/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization
{
    public class TeamCloudNamingStrategy : CamelCaseNamingStrategy
    {
        public TeamCloudNamingStrategy()
            : base(processDictionaryKeys: false, overrideSpecifiedNames: true)
        { }
    }
}
