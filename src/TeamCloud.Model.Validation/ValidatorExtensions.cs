/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation
{
    public static class ValidatorExtensions
    {
        public static bool TryValidate(this IValidatable validatable, out ValidationResult validationResult, IValidatorFactory validatorFactory = null, IServiceProvider serviceProvider = null)
        {
            validationResult = validatable.Validate(validatorFactory, serviceProvider);

            return validationResult.IsValid;
        }

        public static ValidationResult Validate(this IValidatable validatable, IValidatorFactory validatorFactory = null, IServiceProvider serviceProvider = null, bool throwOnNoValidatorFound = false, bool throwOnValidationError = false)
        {
            if (validatable is null)
                throw new ArgumentNullException(nameof(validatable));

            var validators = (validatorFactory ?? ValidatorFactory.DefaultFactory).GetValidators(validatable.GetType(), serviceProvider);

            if (validators.Any())
            {
                var context = new ValidationContext<IValidatable>(validatable);

                var validationResult = validators
                    .Select(validator => validator.Validate(context))
                    .MergeValidationResults();

                if (!validationResult.IsValid && throwOnValidationError)
                    throw validationResult.ToException();

                return validationResult;
            }

            if (throwOnNoValidatorFound)
                throw new NotSupportedException($"Validation of type {validatable.GetType()} is not supported");

            return new ValidationResult();
        }

        public static bool TryValidate<T>(this IValidatable validatable, out ValidationResult validationResult, IServiceProvider serviceProvider = null)
            where T : class, IValidator
        {
            validationResult = validatable.Validate<T>(serviceProvider);

            return validationResult.IsValid;
        }

        public static ValidationResult Validate<T>(this IValidatable validatable, IServiceProvider serviceProvider = null, bool throwOnNotValidable = false, bool throwOnValidationError = false)
            where T : class, IValidator
        {
            if (validatable is null)
                throw new ArgumentNullException(nameof(validatable));

            var validator = serviceProvider is null
                ? Activator.CreateInstance<T>()
                : ActivatorUtilities.CreateInstance<T>(serviceProvider);

            if (validator.CanValidateInstancesOfType(validatable.GetType()))
            {
                var context = new ValidationContext<IValidatable>(validatable);

                var validationResult = validator.Validate(context);

                if (!validationResult.IsValid && throwOnValidationError)
                    throw validationResult.ToException();

                return validationResult;
            }
            else if (throwOnNotValidable)
            {
                throw new NotSupportedException($"Validator or type {typeof(T)} does not support objects of type {validatable.GetType()}");
            }

            return new ValidationResult();
        }

        public static async Task<ValidationResult> ValidateAsync(this IValidatable validatable, IValidatorFactory validatorFactory = null, IServiceProvider serviceProvider = null, bool throwOnNoValidatorFound = false, bool throwOnValidationError = false)
        {
            if (validatable is null)
                throw new ArgumentNullException(nameof(validatable));

            var validators = (validatorFactory ?? ValidatorFactory.DefaultFactory).GetValidators(validatable.GetType(), serviceProvider);

            if (validators.Any())
            {
                var context = new ValidationContext<IValidatable>(validatable);

                var validationTasks = validators
                    .Select(validator => validator.ValidateAsync(context));

                var validationResults = await Task
                    .WhenAll(validationTasks)
                    .ConfigureAwait(false);

                var validationResult = validationResults
                    .MergeValidationResults();

                if (!validationResult.IsValid && throwOnValidationError)
                    throw validationResult.ToException();

                return validationResult;
            }

            if (throwOnNoValidatorFound)
                throw new NotSupportedException($"Validation of type {validatable.GetType()} is not supported");

            return new ValidationResult();
        }

        public static async Task<ValidationResult> ValidateAsync<T>(this IValidatable validatable, IServiceProvider serviceProvider = null, bool throwOnNotValidable = false, bool throwOnValidationError = false)
            where T : class, IValidator
        {
            if (validatable is null)
                throw new ArgumentNullException(nameof(validatable));

            var validator = serviceProvider is null
                ? Activator.CreateInstance<T>()
                : ActivatorUtilities.CreateInstance<T>(serviceProvider);

            if (validator.CanValidateInstancesOfType(validatable.GetType()))
            {
                var context = new ValidationContext<IValidatable>(validatable);

                var validationResult = await validator
                    .ValidateAsync(context)
                    .ConfigureAwait(false);

                if (!validationResult.IsValid && throwOnValidationError)
                    throw validationResult.ToException();

                return validationResult;
            }
            else if (throwOnNotValidable)
            {
                throw new NotSupportedException($"Validator or type {typeof(T)} does not support objects of type {validatable.GetType()}");
            }

            return new ValidationResult();
        }

        private static ValidationResult MergeValidationResults(this IEnumerable<ValidationResult> validationResults)
            => new ValidationResult(validationResults.SelectMany(validationResult => validationResult.Errors));

        public static ValidationException ToException(this ValidationResult validationResult)
            => (validationResult ?? throw new ArgumentNullException(nameof(validationResult))).IsValid ? null : new ValidationException(validationResult.Errors);

        public static ICommandResult ApplyValidationResult(this ICommandResult commandResult, ValidationResult validationResult)
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (validationResult is null)
                throw new ArgumentNullException(nameof(validationResult));

            if (!validationResult.IsValid)
                commandResult.Errors.Add(validationResult.ToException());

            return commandResult;
        }

        public static IRuleBuilderOptions<T, IList<TElement>> MustContainAtLeast<T, TElement>(this IRuleBuilderInitial<T, IList<TElement>> ruleBuilder, int min)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(list => list.Count >= min)
                    .WithMessage("'{PropertyName}' must contain at least " + $"{min} item/s.");

        public static IRuleBuilderOptions<T, IList<TElement>> MustContainAtLeast<T, TElement>(this IRuleBuilderInitial<T, IList<TElement>> ruleBuilder, int min, Func<TElement, bool> predicate, string predicateMessage)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(list => list.Where(predicate).Count() >= min)
                    .WithMessage("'{PropertyName}' must contain at least " + $"{min} item/s succeeding predicate '{predicateMessage}'.");

        public static IRuleBuilderOptions<T, string> MustBeResourceId<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidResourceId)
                    .WithMessage("'{PropertyName}' must be less than 255 characters long and may not contain: " + @"'/', '\\', '?', '#'");

        public static IRuleBuilderOptions<T, string> MustBeAzureRegion<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeAzureRegion)
                    .WithMessage("'{PropertyName}' must be a valid Azure Region. See https://azure.microsoft.com/en-us/global-infrastructure/regions/ for more information on Azure Regions");


        public static IRuleBuilderOptions<T, string> MustBeEmail<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress()
                    .WithMessage("'{PropertyName}' must be a valid email address.");

        public static IRuleBuilderOptions<T, string> MustBeUserIdentifier<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeUserIdentifier)
                    .WithMessage("'{PropertyName}' must be a valid email address, non-empty GUID, or url.");

        public static IRuleBuilderOptions<T, string> MustBeGuid<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeGuid)
                    .WithMessage("'{PropertyName}' must be a valid, non-empty GUID.");

        public static IRuleBuilderOptions<T, Guid> MustBeGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder)
            => ruleBuilder
                .NotEqual(Guid.Empty)
                    .WithMessage("'{PropertyName}' must be a valid, non-empty GUID.");


        public static IRuleBuilderOptions<T, string> MustBeUrl<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeUrl)
                    .WithMessage("'{PropertyName}' must be a valid url.");

        public static IRuleBuilderOptions<T, string> MustBeUserRole<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeUserRole)
                    .WithMessage("'{PropertyName}' must be a valid Role. Valid roles for Project users are 'Owner' and 'Member'. Valid roles for TeamCloud users are 'Admin' and 'Creator'.");

        public static IRuleBuilderOptions<T, string> MustBeProjectUserRole<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeProjectUserRole)
                    .WithMessage("'{PropertyName}' must be a valid Role. Valid roles for Project users are 'Owner' and 'Member'.");

        public static IRuleBuilderOptions<T, string> MustBeTeamCloudUserRole<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeTeamCloudUserRole)
                    .WithMessage("'{PropertyName}' must be a valid Role. Valid roles for TeamCloud users are 'Admin' and 'Creator'.");

        public static IRuleBuilderOptions<T, string> MustBeProviderId<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidProviderId)
                    .WithMessage("'{PropertyName}' must start with a lowercase letter and contain only lowercase letters, numbers, and periods [.] with a length greater than 4 and less than 255");

        public static IRuleBuilderOptions<T, string> MustBeProjectTypeId<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidProjectTypeId)
                    .WithMessage("'{PropertyName}' must start with a lowercase letter and contain only lowercase letters, numbers, and periods [.] with a length greater than 4 and less than 255");

        public static IRuleBuilderOptions<T, string> MustBeFunctionAuthCode<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidFunctionAuthCode)
                    .WithMessage("'{PropertyName}' must contain only base-64 digits [A-Za-z0-9/] (excluding the plus sign (+)), ending in = or ==");

        public static IRuleBuilderOptions<T, string> MustBeVersionString<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
            => ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidVersionString)
                    .WithMessage("'{PropertyName}' be in format v0.0.0 and not include -pre suffix");

        public static IRuleBuilderOptions<T, KeyValuePair<string, string>> MustBeValidTag<T>(this IRuleBuilderInitialCollection<T, KeyValuePair<string, string>> ruleBuilder)
            => ruleBuilder
                .Must(BeValidTag)
                    .WithMessage("'{PropertyName}' must contain valid tag name/values");

        private static bool BeGuid(string guid)
            => !string.IsNullOrEmpty(guid)
            && Guid.TryParse(guid, out var outGuid)
            && !outGuid.Equals(Guid.Empty);

        private static bool BeUrl(string url)
            => !string.IsNullOrEmpty(url)
            && Uri.TryCreate(url, UriKind.Absolute, out var _);

        private static bool BeUserIdentifier(string identifier)
            => !string.IsNullOrWhiteSpace(identifier);

        private static bool BeUserRole(string role)
            => !string.IsNullOrEmpty(role)
            && (Enum.TryParse<TeamCloudUserRole>(role, true, out _)
                || Enum.TryParse<ProjectUserRole>(role, true, out _));

        private static bool BeProjectUserRole(string role)
            => !string.IsNullOrEmpty(role)
            && Enum.TryParse<ProjectUserRole>(role, true, out _);

        private static bool BeTeamCloudUserRole(string role)
            => !string.IsNullOrEmpty(role)
            && Enum.TryParse<TeamCloudUserRole>(role, true, out _);

        private static bool BeAzureRegion(string region)
            => !string.IsNullOrEmpty(region)
            && AzureRegion.IsValid(region);

        // https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/tag-resources#limitations
        private static bool BeValidResourceId(string id)
            => !(string.IsNullOrEmpty(id)
            || id.Length >= 255
            || id.Contains('/', StringComparison.OrdinalIgnoreCase)
            || id.Contains(@"\\", StringComparison.OrdinalIgnoreCase)
            || id.Contains('?', StringComparison.OrdinalIgnoreCase)
            || id.Contains('#', StringComparison.OrdinalIgnoreCase));

        private static bool BeValidTag(KeyValuePair<string, string> tag)
            => BeValidTagName(tag.Key) && BeValidTagValue(tag.Value);

        private static bool BeValidTagName(string key)
            => !(string.IsNullOrEmpty(key)
            || key.Length >= 512
            || key.Contains('<', StringComparison.OrdinalIgnoreCase)
            || key.Contains('>', StringComparison.OrdinalIgnoreCase)
            || key.Contains('%', StringComparison.OrdinalIgnoreCase)
            || key.Contains('&', StringComparison.OrdinalIgnoreCase)
            || key.Contains(@"\\", StringComparison.OrdinalIgnoreCase)
            || key.Contains('?', StringComparison.OrdinalIgnoreCase)
            || key.Contains('/', StringComparison.OrdinalIgnoreCase));

        private static bool BeValidTagValue(string value)
            => !(string.IsNullOrEmpty(value)
            || value.Length >= 256);

        private static readonly Regex validProviderOrProjectTypeId = new Regex(@"^(?:[a-z][a-z0-9]+(?:\.?[a-z0-9]+)+)$");

        private static bool BeValidProviderId(string id)
            => !string.IsNullOrEmpty(id)
            && id.Length > 4
            && id.Length < 255
            && validProviderOrProjectTypeId.IsMatch(id);

        private static bool BeValidProjectTypeId(string id)
            => !string.IsNullOrEmpty(id)
            && id.Length > 4
            && id.Length < 255
            && validProviderOrProjectTypeId.IsMatch(id);

        // https://github.com/Azure/azure-functions-host/blob/dev/src/WebJobs.Script.WebHost/Security/KeyManagement/SecretManager.cs#L592-L603
        private static readonly Regex validFunctionAuthCode = new Regex(@"^([A-Za-z0-9/]{4})*([A-Za-z0-9/]{3}=|[A-Za-z0-9/]{2}==)?$");

        private static bool BeValidFunctionAuthCode(string code)
            => !string.IsNullOrEmpty(code)
            && validFunctionAuthCode.IsMatch(code);

        private static readonly Regex validVersionString = new Regex(@"^v[0-9]+\.[0-9]+\.[0-9]+$");

        private static bool BeValidVersionString(string version)
            => !string.IsNullOrEmpty(version)
            && validVersionString.IsMatch(version);
    }

    internal class AzureRegion
    {
        private static readonly ConcurrentDictionary<string, AzureRegion> regions = new ConcurrentDictionary<string, AzureRegion>();

        #region Americas
        internal static readonly AzureRegion USWest = new AzureRegion("westus");
        internal static readonly AzureRegion USWest2 = new AzureRegion("westus2");
        internal static readonly AzureRegion USCentral = new AzureRegion("centralus");
        internal static readonly AzureRegion USEast = new AzureRegion("eastus");
        internal static readonly AzureRegion USEast2 = new AzureRegion("eastus2");
        internal static readonly AzureRegion USNorthCentral = new AzureRegion("northcentralus");
        internal static readonly AzureRegion USSouthCentral = new AzureRegion("southcentralus");
        internal static readonly AzureRegion USWestCentral = new AzureRegion("westcentralus");
        internal static readonly AzureRegion CanadaCentral = new AzureRegion("canadacentral");
        internal static readonly AzureRegion CanadaEast = new AzureRegion("canadaeast");
        internal static readonly AzureRegion BrazilSouth = new AzureRegion("brazilsouth");
        #endregion

        #region Europe
        internal static readonly AzureRegion EuropeNorth = new AzureRegion("northeurope");
        internal static readonly AzureRegion EuropeWest = new AzureRegion("westeurope");
        internal static readonly AzureRegion UKSouth = new AzureRegion("uksouth");
        internal static readonly AzureRegion UKWest = new AzureRegion("ukwest");
        internal static readonly AzureRegion FranceCentral = new AzureRegion("francecentral");
        internal static readonly AzureRegion FranceSouth = new AzureRegion("francesouth");
        internal static readonly AzureRegion SwitzerlandNorth = new AzureRegion("switzerlandnorth");
        internal static readonly AzureRegion SwitzerlandWest = new AzureRegion("switzerlandwest");
        internal static readonly AzureRegion GermanyNorth = new AzureRegion("germanynorth");
        internal static readonly AzureRegion GermanyWestCentral = new AzureRegion("germanywestcentral");
        internal static readonly AzureRegion NorwayWest = new AzureRegion("norwaywest");
        internal static readonly AzureRegion NorwayEast = new AzureRegion("norwayeast");
        #endregion

        #region Asia
        internal static readonly AzureRegion AsiaEast = new AzureRegion("eastasia");
        internal static readonly AzureRegion AsiaSouthEast = new AzureRegion("southeastasia");
        internal static readonly AzureRegion JapanEast = new AzureRegion("japaneast");
        internal static readonly AzureRegion JapanWest = new AzureRegion("japanwest");
        internal static readonly AzureRegion AustraliaEast = new AzureRegion("australiaeast");
        internal static readonly AzureRegion AustraliaSouthEast = new AzureRegion("australiasoutheast");
        internal static readonly AzureRegion AustraliaCentral = new AzureRegion("australiacentral");
        internal static readonly AzureRegion AustraliaCentral2 = new AzureRegion("australiacentral2");
        internal static readonly AzureRegion IndiaCentral = new AzureRegion("centralindia");
        internal static readonly AzureRegion IndiaSouth = new AzureRegion("southindia");
        internal static readonly AzureRegion IndiaWest = new AzureRegion("westindia");
        internal static readonly AzureRegion KoreaSouth = new AzureRegion("koreasouth");
        internal static readonly AzureRegion KoreaCentral = new AzureRegion("koreacentral");
        #endregion

        #region Middle East and Africa
        internal static readonly AzureRegion UAECentral = new AzureRegion("uaecentral");
        internal static readonly AzureRegion UAENorth = new AzureRegion("uaenorth");
        internal static readonly AzureRegion SouthAfricaNorth = new AzureRegion("southafricanorth");
        internal static readonly AzureRegion SouthAfricaWest = new AzureRegion("southafricawest");
        #endregion

        #region China
        internal static readonly AzureRegion ChinaNorth = new AzureRegion("chinanorth");
        internal static readonly AzureRegion ChinaEast = new AzureRegion("chinaeast");
        internal static readonly AzureRegion ChinaNorth2 = new AzureRegion("chinanorth2");
        internal static readonly AzureRegion ChinaEast2 = new AzureRegion("chinaeast2");
        #endregion

        #region German
        internal static readonly AzureRegion GermanyCentral = new AzureRegion("germanycentral");
        internal static readonly AzureRegion GermanyNorthEast = new AzureRegion("germanynortheast");
        #endregion

        #region Government Cloud
        /// <summary>
        /// U.S. government cloud in Virginia.
        /// </summary>
        internal static readonly AzureRegion GovernmentUSVirginia = new AzureRegion("usgovvirginia");

        /// <summary>
        /// U.S. government cloud in Iowa.
        /// </summary>
        internal static readonly AzureRegion GovernmentUSIowa = new AzureRegion("usgoviowa");

        /// <summary>
        /// U.S. government cloud in Arizona.
        /// </summary>
        internal static readonly AzureRegion GovernmentUSArizona = new AzureRegion("usgovarizona");

        /// <summary>
        /// U.S. government cloud in Texas.
        /// </summary>
        internal static readonly AzureRegion GovernmentUSTexas = new AzureRegion("usgovtexas");

        /// <summary>
        /// U.S. Department of Defense cloud - East.
        /// </summary>
        internal static readonly AzureRegion GovernmentUSDodEast = new AzureRegion("usdodeast");

        /// <summary>
        /// U.S. Department of Defense cloud - Central.
        /// </summary>
        internal static readonly AzureRegion GovernmentUSDodCentral = new AzureRegion("usdodcentral");

        #endregion

        internal static IReadOnlyCollection<AzureRegion> Values
            => regions.Values as IReadOnlyCollection<AzureRegion>;

        internal string Name
        {
            get; private set;
        }

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
        private AzureRegion(string name)
        {
            Name = name.ToLowerInvariant();
            regions.AddOrUpdate(Name, this, (k, v) => v);
        }

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
        internal static bool IsValid(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return regions.ContainsKey(name.Replace(" ", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant());
        }

        public override int GetHashCode()
            => this.Name?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? base.GetHashCode();

        public static bool operator ==(AzureRegion lhs, AzureRegion rhs)
            => (lhs is null) ? rhs is null : lhs.Equals(rhs);

        public static bool operator !=(AzureRegion lhs, AzureRegion rhs)
            => !(lhs == rhs);

        public override bool Equals(object obj)
        {
            if (!(obj is AzureRegion))
                return false;

            if (object.ReferenceEquals(obj, this))
                return true;

            AzureRegion rhs = (AzureRegion)obj;

            if (Name is null)
                return rhs.Name is null;

            return Name.Equals(rhs.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString() => this.Name;
    }
}
