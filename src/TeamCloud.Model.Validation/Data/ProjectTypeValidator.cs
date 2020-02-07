/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProjectTypeValidator : AbstractValidator<ProjectType>
    {
        public ProjectTypeValidator()
        {
            RuleFor(obj => obj.Id).MustBeResourcId();
            RuleFor(obj => obj.Region).MustBeAzureRegion();

            RuleFor(obj => obj.Subscriptions)
                .MustContainAtLeast(3)
                .ForEach(sub => sub.MustBeGuid());

            RuleFor(obj => obj.Providers)
                .MustContainAtLeast(1)
                .ForEach(provider => provider.SetValidator(new ProjectTypeProviderValidator()));
        }
    }
}
