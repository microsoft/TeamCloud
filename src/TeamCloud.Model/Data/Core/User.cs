/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public interface IUser : IProperties
    {
        string Id { get; set; }

        UserType UserType { get; set; }

        TeamCloudUserRole Role { get; set; }

        IList<ProjectMembership> ProjectMemberships { get; set; }
    }
}
