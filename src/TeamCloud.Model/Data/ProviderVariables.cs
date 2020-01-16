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
    public sealed class ProviderVariables
    {
        public string ProviderId { get; set; }

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }

    public sealed class ProviderVariablesValidator : AbstractValidator<ProviderVariables>
    {
        public ProviderVariablesValidator()
        {
            RuleFor(obj => obj.ProviderId).NotEmpty();
            RuleFor(obj => obj.Variables).NotEmpty();
        }
    }
}