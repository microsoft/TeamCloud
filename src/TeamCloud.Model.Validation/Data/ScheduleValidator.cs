/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ScheduleValidator : Validator<Schedule>
    {
        public ScheduleValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.Id)
                .MustBeGuid();

            RuleFor(obj => obj.ProjectId)
                .MustBeGuid();

            RuleFor(obj => obj.UtcHour)
                .InclusiveBetween(0, 23);

            RuleFor(obj => obj.UtcMinute)
                .InclusiveBetween(0, 59);

            RuleFor(obj => obj.DaysOfWeek)
                .NotEmpty();

            RuleFor(obj => obj.Creator)
                .MustBeGuid();
        }
    }
}
