﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Validation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators;

public sealed class ProjectUserDefinitionValidator : Validator<UserDefinition>
{
    public ProjectUserDefinitionValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        RuleFor(obj => obj.Identifier).MustBeUserIdentifier();
        RuleFor(obj => obj.Role).MustBeProjectUserRole();
    }
}
