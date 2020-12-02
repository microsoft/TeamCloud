using FluentValidation;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public class ProjectComponentDefinitionValidator : AbstractValidator<ProjectComponentDefinition>
    {
        public ProjectComponentDefinitionValidator()
        {
            RuleFor(obj => obj.TemplateId)
                .MustBeGuid();

            RuleFor(obj => obj.DeploymentScopeId)
                .MustBeGuid()
                .When(obj => !string.IsNullOrWhiteSpace(obj.DeploymentScopeId));

            RuleFor(obj => obj.InputJson)
                .MustBeJson()
                .When(obj => !string.IsNullOrWhiteSpace(obj.InputJson));
        }
    }
}
