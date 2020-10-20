/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ComponentDocumentValidator : AbstractValidator<ComponentDocument>
    {
        public ComponentDocumentValidator()
        {
            RuleFor(obj => obj.Id)
                .MustBeGuid();

            RuleFor(obj => obj.ProviderId)
                .MustBeProviderId();

            RuleFor(obj => obj.ProjectId)
                .MustBeGuid();

            RuleFor(obj => obj.RequestedBy)
                .MustBeGuid();

            RuleFor(obj => obj.OfferId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must((obj, id) => id.StartsWith($"{obj.ProviderId}.", StringComparison.Ordinal))
                .WithMessage(obj => "'{PropertyName}' must begin with the providerId followed by a period " + $"({obj.ProviderId}.)");

            RuleFor(obj => obj.ValueJson)
                .NotNull();
        }
    }
}
