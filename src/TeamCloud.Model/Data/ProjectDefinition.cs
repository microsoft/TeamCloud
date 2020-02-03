// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System;
// using System.Collections.Generic;
// using FluentValidation;
// using Newtonsoft.Json;

// namespace TeamCloud.Model.Data
// {
//     public sealed class ProjectDefinition : IIdentifiable, IContainerDocument, IEquatable<ProjectDefinition>
//     {
//         //public int Version { get; set; }

//         [JsonIgnore]
//         public List<string> UniqueKeys => new List<string> { };

//         public string PartitionKey => Constants.CosmosDb.TeamCloudInstanceId;

//         public Guid Id { get; set; }

//         public string Region { get; set; }

//         public List<Guid> Subscriptions { get; set; }

//         public int? SubscriptionCapacity { get; set; } = 10;

//         public string ResourceGroupNamePrefix { get; set; }

//         public List<string> Providers { get; set; } // + Properties?

//         public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

//         public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

//         public bool Equals(ProjectDefinition other) => Id.Equals(other.Id);
//     }

//     public sealed class ProjectDefinitionValidator : AbstractValidator<ProjectDefinition>
//     {
//         public ProjectDefinitionValidator()
//         {
//             RuleFor(obj => obj.Id).NotEmpty();
//             RuleFor(obj => obj.Region).NotEmpty();
//             RuleFor(obj => obj.Subscriptions).NotEmpty();
//             RuleFor(obj => obj.Subscriptions).Must(obj => obj.Count >= 3);
//             RuleFor(obj => obj.Providers).NotEmpty();
//             RuleFor(obj => obj.Providers).Must(obj => obj.Count >= 1);

//         }
//     }
// }
