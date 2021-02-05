/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

// using System;
// using FluentValidation;
// using TeamCloud.Model.Data;

// namespace TeamCloud.Model.Validation.Data
// {
//     public sealed class ComponentRequestValidator : AbstractValidator<ComponentRequest>
//     {
//         public ComponentRequestValidator()
//         {
//             // RuleFor(obj => obj.Id)
//             //     .MustBeGuid();

//             // RuleFor(obj => obj.ProviderId)
//             //     .MustBeProviderId();

//             // RuleFor(obj => obj.ProjectId)
//             //     .MustBeGuid();

//             // RuleFor(obj => obj.RequesterId)
//             //     .MustBeGuid();

//             // RuleFor(obj => obj.OfferId)
//             //     .Cascade(CascadeMode.Stop)
//             //     .NotEmpty()
//             //     .Must((obj, id) => id.StartsWith($"{obj.ProviderId}.", StringComparison.Ordinal))
//             //     .WithMessage(obj => "'{PropertyName}' must begin with the providerId followed by a period " + $"({obj.ProviderId}.)");
//         }
//     }
// }
