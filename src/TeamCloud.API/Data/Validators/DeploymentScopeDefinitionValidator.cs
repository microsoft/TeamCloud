/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators
{
    public sealed class DeploymentScopeDefinitionValidator : Validator<DeploymentScopeDefinition>
    {
        private readonly IAdapterProvider adapterProvider;

        public DeploymentScopeDefinitionValidator(IValidatorProvider validatorProvider, IAdapterProvider adapterProvider) : base(validatorProvider)
        {
            this.adapterProvider = adapterProvider ?? throw new System.ArgumentNullException(nameof(adapterProvider));

            RuleFor(obj => obj.DisplayName)
                .NotEmpty();

            RuleFor(obj => obj.Type)
                .Must(type => this.adapterProvider.GetAdapter(type) is not null)
                .WithMessage("{PropertyName} is out of range.");

            RuleFor(obj => obj.InputData)
                .NotEmpty();
            
            RuleFor(obj => obj)
                .MustAsync(ValidInputDataAsync)
                .WithMessage("{PropertyName} must match schema.");
        }

        private async Task<bool> ValidInputDataAsync(DeploymentScopeDefinition deploymentScopeDefinition, CancellationToken cancellationToken = default)
        {
            var json = string.IsNullOrEmpty(deploymentScopeDefinition.InputData)
                ? null : JToken.Parse(deploymentScopeDefinition.InputData);

            var adapter = adapterProvider.GetAdapter(deploymentScopeDefinition.Type);

            if (adapter is not null)
            { 
                var schemaJson = await adapter
                    .GetInputDataSchemaAsync()
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(schemaJson))
                    return json.IsValid(JSchema.Parse(schemaJson));
            }

            return false;
        }
    }
}
