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
    public interface IProject : ITags, IProperties
    {
        string Id { get; set; }

        string Name { get; set; }

        AzureResourceGroup ResourceGroup { get; set; }

        AzureKeyVault KeyVault { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public interface IProject<TUser> : IProject
        where TUser : IUser
    {
        IList<TUser> Users { get; set; }
    }
}
