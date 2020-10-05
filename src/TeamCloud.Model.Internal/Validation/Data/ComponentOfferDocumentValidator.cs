/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ComponentOfferDocumentValidator : AbstractValidator<ComponentOfferDocument>
    {
        public ComponentOfferDocumentValidator()
        {
            RuleFor(obj => obj.ProviderId)
                .MustBeProviderId();

            RuleFor(obj => obj.Id)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .Must((obj, id) => id.StartsWith($"{obj.ProviderId}.", StringComparison.Ordinal))
                .WithMessage(obj => "'{PropertyName}' must begin with the providerId followed by a period " + $"({obj.ProviderId}.)");

            RuleFor(obj => obj.InputJsonSchema)
                .NotEmpty();
        }
    }
}
