/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AzureResourceGroup : IIdentifiable, IEquatable<AzureResourceGroup>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubscriptionId { get; set; } = Guid.Empty;

        public string ResourceGroupId { get; set; }

        public string ResourceGroupName { get; set; }

        public string Region { get; set; }

        public bool Equals(AzureResourceGroup other) => Id.Equals(other.Id);
    }

    public sealed class AzureResourceGroupValidator : AbstractValidator<AzureResourceGroup>
    {
        public AzureResourceGroupValidator()
        {
            RuleFor(obj => obj.SubscriptionId).NotEmpty();
            RuleFor(obj => obj.ResourceGroupName).NotEmpty();
            RuleFor(obj => obj.Region).NotEmpty();
        }
    }
}