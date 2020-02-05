/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation
{
    public sealed class ProviderValidator : AbstractValidator<Provider>
    {
        public ProviderValidator()
        {
            RuleFor(obj => obj.Id).MustBeGuid();
            RuleFor(obj => obj.Url).MustBeUrl();
            RuleFor(obj => obj.AuthCode).NotEmpty();
        }
    }

    // [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    // public sealed class ProviderDependencies
    // {
    //     public List<string> Create { get; set; } = new List<string>();

    //     public List<string> Init { get; set; } = new List<string>();
    // }
}
