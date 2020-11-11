/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class ProjectTemplateDefinitionValidator : AbstractValidator<ProjectTemplateDefinition>
    {
        public ProjectTemplateDefinitionValidator()
        {
            // RuleFor(obj => obj.Name).NotEmpty();
            // RuleFor(obj => obj.Users)
            //     .ForEach(user => user.SetValidator(new ProjectUserDefinitionValidator()));
        }
    }
}
