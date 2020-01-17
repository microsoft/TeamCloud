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
    public sealed class AzureIdentity : IIdentifiable, IEquatable<AzureIdentity>
    {
        public Guid Id { get; set; }

        public string AppId { get; set; }

        public string Secret { get; set; }

        public bool Equals(AzureIdentity other) => Id.Equals(other.Id);
    }

    public sealed class AzureIdentityValidator : AbstractValidator<AzureIdentity>
    {
        public AzureIdentityValidator()
        {
            RuleFor(obj => obj.Id).NotEqual(Guid.Empty);
            RuleFor(obj => obj.AppId).NotEmpty();
            RuleFor(obj => obj.Secret).NotEmpty();
        }
    }
}