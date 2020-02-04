/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TeamCloud.Model.Data
{
    internal static class Validation
    {
        internal static bool BeGuid(string guid)
            => !string.IsNullOrEmpty(guid) && Guid.TryParse(guid, out var _);

        internal static bool BeAzureRegion(string region)
            => !string.IsNullOrEmpty(region) && AzureRegion.IsValid(region);

        internal static bool BeValidResourceId(string id)
            => !(string.IsNullOrEmpty(id) || id.Length >= 255 || id.Contains('/') || id.Contains(@"\\") || id.Contains('?') || id.Contains('#'));
    }

    internal partial class AzureRegion
    {
        private static ConcurrentDictionary<string, AzureRegion> regions = new ConcurrentDictionary<string, AzureRegion>();

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

        private AzureRegion(string name)
        {
            Name = name.ToLowerInvariant();
            regions.AddOrUpdate(Name, this, (k, v) => v);
        }

        internal static bool IsValid(string name)
            => regions.ContainsKey(name.Replace(" ", "").ToLowerInvariant());

        public override int GetHashCode() => this.Name.GetHashCode();

        public static bool operator ==(AzureRegion lhs, AzureRegion rhs)
            => (object.ReferenceEquals(lhs, null))
                ? object.ReferenceEquals(rhs, null)
                : lhs.Equals(rhs);

        public static bool operator !=(AzureRegion lhs, AzureRegion rhs) => !(lhs == rhs);

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
