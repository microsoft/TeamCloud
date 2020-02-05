/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation
{
    public sealed class ProjectTypeValidator : AbstractValidator<ProjectType>
    {
        public ProjectTypeValidator()
        {
            RuleFor(obj => obj.Id).MustBeValidResourcId();
            RuleFor(obj => obj.Region).MustBeAzureRegion();

            RuleFor(obj => obj.Subscriptions).NotEmpty();
            RuleFor(obj => obj.Subscriptions).Must(obj => obj.Count >= 3);

            RuleFor(obj => obj.Providers).NotEmpty();
            RuleFor(obj => obj.Providers).Must(obj => obj.Count >= 1);
        }
    }
}
