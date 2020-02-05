/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class UserDefinition
    {
        public string Email { get; set; }

        public string Role { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public sealed class UserDefinitionValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionValidator()
        {
            RuleFor(obj => obj.Email).MustBeEmail();
            RuleFor(obj => obj.Role).MustBeUserRole();
        }
    }
}
