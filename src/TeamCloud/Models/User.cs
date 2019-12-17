/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public interface IUser
    {
        string Id { get; set; }
        
        Dictionary<string,string> Tags { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TeamCloudUser : IUser
    {
        public string Id { get; set; }
        
        public TeamCloudUserRole Role { get; set; }
        
        public Dictionary<string, string> Tags { get; set; }
    }

    public enum TeamCloudUserRole
    {
        None,    // 0
        Creator, // 1
        Admin,   // 2
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProjectUser : IUser
    {
        public string Id { get; set; }
        
        public ProjectUserRole Role { get; set; }
        
        public Dictionary<string, string> Tags { get; set; }
    }

    public enum ProjectUserRole
    {
        None,   // 0
        Member, // 1 
        Owner   // 2
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserDefinition<T> where T : System.Enum
    {
        public string Email { get; set; }
        
        public T Role { get; set; }
        
        public Dictionary<string,string> Tags { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TeamCloudUserDefinition : UserDefinition<TeamCloudUserRole> { }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProjectUserDefinition : UserDefinition<ProjectUserRole> {}
}
