/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation
{
    public class ProjectTypeProviderValidator : AbstractValidator<ProjectTypeProvider>
    {
        public ProjectTypeProviderValidator()
        {
            RuleFor(obj => obj.Id).NotEmpty();
        }
    }
}
