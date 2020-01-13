/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserDefinition
    {
        public string Email { get; set; }

        public string Role { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public sealed class UserDefinitionValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionValidator()
        {
            RuleFor(obj => obj.Email).NotEmpty();
            RuleFor(obj => obj.Role).NotEmpty();
            RuleFor(obj => obj.Tags).NotNull();
        }
    }
}