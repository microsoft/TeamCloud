/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;

namespace TeamCloud.Model.Data
{
    public sealed class ProjectType : IContainerDocument, IEquatable<ProjectType>
    {
        [JsonIgnore]
        public List<string> UniqueKeys => new List<string> { };

        public string PartitionKey => Constants.CosmosDb.TeamCloudInstanceId;

        public string Id { get; set; }

        public bool Default { get; set; }

        public string Region { get; set; }

        public List<Guid> Subscriptions { get; set; }

        public int? SubscriptionCapacity { get; set; } = 10;

        public string ResourceGroupNamePrefix { get; set; }

        public List<ProjectTypeProvider> Providers { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public bool Equals(ProjectType other) => Id.Equals(other.Id);
    }

    public sealed class ProjectTypeValidator : AbstractValidator<ProjectType>
    {
        public ProjectTypeValidator()
        {
            RuleFor(obj => obj.Id).Must(Validation.BeValidResourceId);
            RuleFor(obj => obj.Region).Must(Validation.BeAzureRegion);

            RuleFor(obj => obj.Subscriptions).NotEmpty();
            RuleFor(obj => obj.Subscriptions).Must(obj => obj.Count >= 3);

            RuleFor(obj => obj.Providers).NotEmpty();
            RuleFor(obj => obj.Providers).Must(obj => obj.Count >= 1);
        }
    }
}
