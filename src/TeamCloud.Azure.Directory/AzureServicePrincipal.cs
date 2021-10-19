/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace TeamCloud.Azure.Directory
{
    public sealed class AzureServicePrincipal
    {
        public Guid ObjectId { get; internal set; }
        public Guid ApplicationId { get; internal set; }
        public Guid TenantId { get; internal set; }
        public string Name { get; internal set; }
        public string Password { get; internal set; }
        public DateTime? ExpiresOn { get; internal set; }

        public ClaimsIdentity ToClaimsIdentity(string authenticationType = null)
        {
            const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";

            var claims = new List<Claim>()
            {
                new Claim(ObjectIdClaimType, ObjectId.ToString()),
                new Claim(TenantIdClaimType, TenantId.ToString()),
                new Claim(ClaimTypes.Name, Name)
            };

            return new ClaimsIdentity(claims, authenticationType);
        }
    }
}
