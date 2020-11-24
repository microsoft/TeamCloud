using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class OrganizationValidator : AbstractValidator<Organization>
    {
        public OrganizationValidator()
        {
            RuleFor(obj => obj.DisplayName)
                .NotEmpty();

            RuleFor(obj => obj.SubscriptionId)
                .MustBeGuid();

            RuleFor(obj => obj.Location)
                .NotEmpty();
        }
    }
}
