/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class RepositoryDefinitionValidator : AbstractValidator<RepositoryDefinition>
    {
        public RepositoryDefinitionValidator()
        {
            RuleFor(obj => obj.Url).MustBeUrl();
        }
    }
}
