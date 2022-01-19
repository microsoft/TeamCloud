/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Reflection;
using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation;

public static class ValidationExtensions
{
    public static IValidatorProviderConfig RegisterModelValidators(this IValidatorProviderConfig validatorProviderConfig)
        => validatorProviderConfig.Register(Assembly.GetExecutingAssembly());

    public static IRuleBuilderOptions<T, string> MustBeUserRole<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
        => ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeUserRole)
                .WithMessage("'{PropertyName}' must be a valid Role. Valid roles for Project users are 'Owner', 'Admin', 'Member', 'None'.");

    private static bool BeUserRole(string role)
        => !string.IsNullOrEmpty(role)
        && (Enum.TryParse<OrganizationUserRole>(role, true, out _) || Enum.TryParse<ProjectUserRole>(role, true, out _));

    public static IRuleBuilderOptions<T, string> MustBeProjectUserRole<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
        => ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeProjectUserRole)
                .WithMessage("'{PropertyName}' must be a valid Role. Valid roles for Project users are 'Owner', 'Admin', 'Member', and 'None'.");

    private static bool BeProjectUserRole(string role)
        => !string.IsNullOrEmpty(role)
        && Enum.TryParse<ProjectUserRole>(role, true, out _);


    public static IRuleBuilderOptions<T, string> MustBeOrganizationUserRole<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
        => ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeOrganizationUserRole)
                .WithMessage("'{PropertyName}' must be a valid Role. Valid roles for Organization users are 'Owner', 'Admin', 'Member' and 'None'.");


    private static bool BeOrganizationUserRole(string role)
        => !string.IsNullOrEmpty(role)
        && Enum.TryParse<OrganizationUserRole>(role, true, out _);
}
