/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProviderDataValidator : AbstractValidator<ProviderData>
    {
        public ProviderDataValidator()
        {
            // RuleFor(obj => obj.ProviderId).MustBeProviderId();

            // RuleFor(obj => obj.ProjectId)
            //     .MustBeGuid()
            //     .When(obj => obj.Scope == ProviderDataScope.Project)
            //     .WithMessage("'{PropertyName}' must be a non-empty GUID when 'scope' = 'Project'.");

            // RuleFor(obj => obj.ProjectId)
            //     .Empty()
            //     .When(obj => obj.Scope == ProviderDataScope.System)
            //     .WithMessage("'{PropertyName}' must not have a value when 'scope' = 'System'.");

            RuleFor(obj => obj.Name)
                .NotEmpty();

            RuleFor(obj => obj.Location)
                .MustBeUrl()
                .When(obj => obj.DataType == ProviderDataType.Service)
                .WithMessage("'{PropertyName}' must be a valie url when 'dataType' = 'Service'."); ;
        }
    }
}
