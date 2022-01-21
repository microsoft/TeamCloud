/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Data;

public sealed class ProjectIdentityValidator : Validator<ProjectIdentity>
{
    public ProjectIdentityValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
    }
}
