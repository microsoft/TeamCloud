/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Data;

public sealed class ComponentValidator : Validator<Component>
{
    public ComponentValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        RuleFor(obj => obj.Id)
            .MustBeGuid();

        RuleFor(obj => obj.ProjectId)
            .MustBeGuid();

        RuleFor(obj => obj.Creator)
            .MustBeGuid();
    }
}
