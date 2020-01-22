/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Provider
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public string AuthCode { get; set; }

        public bool Optional { get; set; }

        public ProviderDependencies Dependencies { get; set; }

        public List<string> Events { get; set; } = new List<string>();

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }

    public sealed class ProviderValidator : AbstractValidator<Provider>
    {
        public ProviderValidator()
        {
            RuleFor(obj => obj.Id).NotEmpty();
            RuleFor(obj => obj.Url).NotEmpty();
            RuleFor(obj => obj.AuthCode).NotEmpty();
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ProviderDependencies
    {
        public List<string> Create { get; set; } = new List<string>();

        public List<string> Init { get; set; } = new List<string>();
    }
}
