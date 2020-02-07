/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class AzureIdentityValidator : AbstractValidator<AzureIdentity>
    {
        public AzureIdentityValidator()
        {
            RuleFor(obj => obj.Id).MustBeGuid();
            RuleFor(obj => obj.AppId).NotEmpty();
            RuleFor(obj => obj.Secret).NotEmpty();
        }
    }
}
