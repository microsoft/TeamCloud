/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class ProjectDefinitionValidator : AbstractValidator<ProjectDefinition>
    {
        public ProjectDefinitionValidator()
        {
            RuleFor(obj => obj.DisplayName).NotEmpty();
            RuleFor(obj => obj.Template).MustBeGuid(); //TODO: May not need to require this if there is a default template
            RuleFor(obj => obj.TemplateInput).NotEmpty();
            RuleFor(obj => obj.Users)
                .ForEach(user => user.SetValidator(new ProjectUserDefinitionValidator()));
        }
    }
}
