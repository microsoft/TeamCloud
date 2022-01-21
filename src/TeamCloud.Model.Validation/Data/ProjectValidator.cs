/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProjectValidator : Validator<Project>
    {
        public ProjectValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.DisplayName)
                .NotEmpty();

            // RuleFor(obj => obj.Type)
            //     .NotEmpty();

            // RuleFor(obj => obj.Users)
            //     .NotEmpty();
        }
    }
}
