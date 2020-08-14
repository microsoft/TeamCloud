/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Model.Internal.Validation.Data
{
    public sealed class ProjectIdentityValidator : AbstractValidator<ProjectIdentity>
    {
        public ProjectIdentityValidator()
        {
            RuleFor(obj => obj.Id).MustBeGuid();
            RuleFor(obj => obj.ApplicationId).NotEmpty();
            RuleFor(obj => obj.Secret).NotEmpty();
        }
    }
}
