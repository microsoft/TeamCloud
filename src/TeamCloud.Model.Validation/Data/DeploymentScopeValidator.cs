/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class DeploymentScopeValidator : Validator<DeploymentScope>
    {
        public DeploymentScopeValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.Organization)
                .MustBeGuid();

            RuleFor(obj => obj.DisplayName)
                .NotEmpty();

            RuleFor(obj => obj)
                .Must(ValidInputData)
                .WithMessage("Input data and schema must match.");
        }

        private bool ValidInputData(DeploymentScope deploymentScope)
        {
            var json = string.IsNullOrEmpty(deploymentScope.InputData)
                ? null : JToken.Parse(deploymentScope.InputData);

            var schema = string.IsNullOrEmpty(deploymentScope.InputDataSchema)
                ? null : JSchema.Parse(deploymentScope.InputDataSchema);

            return schema is null
                ? true : json.IsValid(schema);
        }
    }
}
