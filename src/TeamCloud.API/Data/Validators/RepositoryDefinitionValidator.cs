/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators
{
    public sealed class RepositoryDefinitionValidator : Validator<RepositoryDefinition>
    {
        public RepositoryDefinitionValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.Url).MustBeUrl();
        }
    }
}
