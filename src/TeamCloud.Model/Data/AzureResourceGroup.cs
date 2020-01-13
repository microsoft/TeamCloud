/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace TeamCloud.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AzureResourceGroup : Identifiable, IEquatable<AzureResourceGroup>
    {
        public Guid Id { get; set; }

        public string SubscriptionId { get; set; }

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