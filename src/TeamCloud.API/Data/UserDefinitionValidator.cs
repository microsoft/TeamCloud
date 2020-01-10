using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Model;

namespace TeamCloud.API.Data
{
    public class UserDefinitionValidator : AbstractValidator<UserDefinition>
    {
        private static readonly string[] ValidProjectRoles = new string[]
        {
            UserRoles.Project.Owner,
            UserRoles.Project.Member
        };

        public UserDefinitionValidator()
        {
            RuleFor(obj => obj.Email).NotEmpty()
                .WithMessage("email is required");

            RuleFor(obj => obj.Email).EmailAddress()
                .WithMessage("email must contain a valid email address");

            RuleFor(obj => obj.Role).Must(role => ValidProjectRoles.Contains(role))
                .WithMessage($"invalid role detected - valid rules: {string.Join(", ", ValidProjectRoles)}");
        }
    }
}
