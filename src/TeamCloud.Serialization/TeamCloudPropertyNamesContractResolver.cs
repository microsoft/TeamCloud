/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization
{
    public class TeamCloudPropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
    {
        public TeamCloudPropertyNamesContractResolver()
        {
            NamingStrategy = new TeamCloudNamingStrategy();
        }
    }
}
