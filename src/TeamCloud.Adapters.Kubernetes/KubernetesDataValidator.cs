using FluentValidation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Adapters.Kubernetes
{
    public sealed class KubernetesDataValidator : Validator<KubernetesData>
    {
        public KubernetesDataValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.Namespace)
                .NotEmpty();

            RuleFor(obj => obj.Yaml)
                .NotEmpty();

            RuleFor(obj => obj.File)
                .NotEmpty()
                .When(obj => obj.Source == KubernetesConfigurationSource.File);
        }
    }
}
