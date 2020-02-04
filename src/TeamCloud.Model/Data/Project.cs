/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Project : IIdentifiable, IContainerDocument, IEquatable<Project>
    {
        public string PartitionKey => TeamCloudId;

        [JsonIgnore]
        public List<string> UniqueKeys => new List<string> { "/name" };

        public Guid Id { get; set; }

        public string Name { get; set; }

        public ProjectType Type { get; set; }

        public AzureIdentity Identity { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public string TeamCloudId { get; set; }

        public string TeamCloudApplicationInsightsKey { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, Dictionary<string, string>> ProviderProperties { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public bool Equals(Project other) => Id.Equals(other.Id);
    }

    public sealed class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(obj => obj.Name).NotEmpty();
            RuleFor(obj => obj.Users).NotEmpty();
            RuleFor(obj => obj.Type).NotEmpty();
        }
    }
}
