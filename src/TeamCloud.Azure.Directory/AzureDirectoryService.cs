/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Rest.Azure.OData;
using TeamCloud.Http;

namespace TeamCloud.Azure.Directory
{
    public interface IAzureDirectoryService
    {
        Task<Guid?> GetUserIdAsync(string identifier);

        Task<Guid?> GetGroupIdAsync(string identifier);

        Task<AzureServicePrincipal> CreateServicePrincipalAsync(string servicePrincipalName, string password = null);

        Task<AzureServicePrincipal> GetServicePrincipalAsync(string servicePrincipalName);

        Task DeleteServicePrincipalAsync(string servicePrincipalName);
    }

    public class AzureDirectoryService : IAzureDirectoryService
    {
        private readonly IAzureSessionService azureSessionService;

        public AzureDirectoryService(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        public async Task<Guid?> GetUserIdAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            using var graphRbacManagementClient = azureSessionService
                .CreateClient<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint);

            if (identifier.IsEMail())
            {
                var domains = await graphRbacManagementClient.Domains
                    .ListAsync()
                    .ConfigureAwait(false);

                var hasVerifiedDomain = domains
                    .Where(d => d.IsVerified.HasValue && d.IsVerified.Value)
                    .Any(d => identifier.EndsWith($"@{d.Name}", StringComparison.OrdinalIgnoreCase));

                if (!hasVerifiedDomain)
                {
                    var defaultDomain = domains
                        .First(d => d.IsDefault.HasValue && d.IsDefault.Value);

                    identifier = $"{identifier.Replace("@", "_", StringComparison.OrdinalIgnoreCase)}#EXT#@{defaultDomain.Name}";
                }
            }

            // assume user first
            var userInner = await graphRbacManagementClient.Users
                .GetAsync(identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(userInner?.ObjectId))
                return Guid.Parse(userInner.ObjectId);

            // otherwise try to find a service pricipal
            var principalPage = await graphRbacManagementClient.ServicePrincipals
                .ListAsync(new ODataQuery<ServicePrincipalInner>(sp => sp.ObjectId == identifier || sp.ServicePrincipalNames.Contains(identifier)))
                .ConfigureAwait(false);

            var principalInner = principalPage
                .FirstOrDefault(sp => sp.ObjectId == identifier || sp.ServicePrincipalNames.Any(n => n.Equals(identifier, StringComparison.OrdinalIgnoreCase)));

            while (principalInner is null && !string.IsNullOrEmpty(principalPage?.NextPageLink))
            {
                principalPage = await graphRbacManagementClient.ServicePrincipals
                    .ListNextAsync(principalPage.NextPageLink)
                    .ConfigureAwait(false);

                principalInner = principalPage
                    .FirstOrDefault(sp => sp.ObjectId == identifier || sp.ServicePrincipalNames.Any(n => n.Equals(identifier, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(principalInner?.ObjectId))
                return Guid.Parse(principalInner.ObjectId);

            // not a user name or objectId, and not a service pricipal name, appId, or objectId
            return null;
        }

        public Task<Guid?> GetGroupIdAsync(string identifier)
        {
            throw new NotImplementedException();
        }

        private static string CreateServicePrincipalPassword()
        {
            using var cryptRNG = new RNGCryptoServiceProvider();

            byte[] tokenBuffer = new byte[20];
            cryptRNG.GetBytes(tokenBuffer);

            return Convert.ToBase64String(tokenBuffer);
        }

        private static string SanitizeServicePrincipalName(string servicePrincipalName)
        {
            const string ServicePrincipalNamePrefix = "TeamCloud/";

            if (servicePrincipalName.StartsWith(ServicePrincipalNamePrefix, StringComparison.OrdinalIgnoreCase))
                servicePrincipalName = ServicePrincipalNamePrefix + servicePrincipalName.Substring(ServicePrincipalNamePrefix.Length);
            else
                servicePrincipalName = ServicePrincipalNamePrefix + servicePrincipalName;

            return servicePrincipalName.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        // public async Task<AzureServicePrincipal> CreateServicePrincipalAsync(string servicePrincipalName, string password = null)
        // {
        //     if (string.IsNullOrWhiteSpace(servicePrincipalName))
        //         throw new ArgumentException("Must not NULL or WHITESPACE", nameof(servicePrincipalName));

        //     servicePrincipalName = SanitizeServicePrincipalName(servicePrincipalName);

        //     using var graphRbacManagementClient = azureSessionService
        //         .CreateClient<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint);

        //     password ??= CreateServicePrincipalPassword();

        //     var applicationCreateParameters = new ApplicationCreateParameters()
        //     {
        //         DisplayName = servicePrincipalName,
        //         IdentifierUris = new List<string> { $"http://{servicePrincipalName}" },
        //         Homepage = $"http://{servicePrincipalName}",
        //         PasswordCredentials = new List<PasswordCredential> {
        //             new PasswordCredential {
        //                 StartDate = DateTime.UtcNow,
        //                 EndDate = DateTime.UtcNow.AddYears(1),
        //                 KeyId = Guid.NewGuid().ToString(),
        //                 Value = password,
        //                 // CustomKeyIdentifier = Convert.ToBase64String(servicePrincipal.ObjectId.ToByteArray())
        //             }
        //         },
        //         RequiredResourceAccess = new List<RequiredResourceAccess> {
        //             new RequiredResourceAccess {
        //                 ResourceAppId = "00000003-0000-0000-c000-000000000000",
        //                 ResourceAccess = new List<ResourceAccess> {
        //                     new ResourceAccess {
        //                         Id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
        //                         Type = "Scope"
        //                     }
        //                 }
        //             }
        //         }
        //     };

        //     var application = await graphRbacManagementClient.Applications
        //         .CreateAsync(applicationCreateParameters)
        //         .ConfigureAwait(false);

        //     var servicePrincipalId = await graphRbacManagementClient.Applications
        //         .GetServicePrincipalsIdByAppIdAsync(application.AppId)
        //         .ConfigureAwait(false);

        //     var servicePrincipal = await graphRbacManagementClient.ServicePrincipals
        //         .GetAsync(servicePrincipalId.Value)
        //         .ConfigureAwait(false);

        //     var azureServicePrincipal = new AzureServicePrincipal()
        //     {
        //         ObjectId = Guid.Parse(servicePrincipal.ObjectId),
        //         ApplicationId = Guid.Parse(servicePrincipal.AppId),
        //         Name = servicePrincipal.ServicePrincipalNames.FirstOrDefault(),
        //         Password = password
        //     };

        //     var expiresOn = servicePrincipal.PasswordCredentials.FirstOrDefault()?.EndDate;

        //     if (expiresOn.HasValue)
        //         azureServicePrincipal.ExpiresOn = expiresOn;

        //     return azureServicePrincipal;
        // }


        public async Task<AzureServicePrincipal> CreateServicePrincipalAsync(string servicePrincipalName, string password = null)
        {
            if (string.IsNullOrWhiteSpace(servicePrincipalName))
                throw new ArgumentException("Must not NULL or WHITESPACE", nameof(servicePrincipalName));

            servicePrincipalName = SanitizeServicePrincipalName(servicePrincipalName);

            try
            {
                var servicePrincipal = await azureSessionService.CreateSession()
                    .ServicePrincipals
                    .Define(servicePrincipalName)
                        .WithNewApplication($"http://{servicePrincipalName}")
                        .CreateAsync()
                        .ConfigureAwait(false);

                return await ResetServicePrincipalAsync(servicePrincipalName, password)
                    .ConfigureAwait(false);
            }
            catch
            {
                // we created the service principal as part of this call
                // as the password set operation failed we will try
                // to clean up our mess

                await DeleteServicePrincipalAsync(servicePrincipalName)
                    .ConfigureAwait(false);

                throw;
            }
        }

        public async Task<AzureServicePrincipal> ResetServicePrincipalAsync(string servicePrincipalName, string password = null)
        {
            if (string.IsNullOrWhiteSpace(servicePrincipalName))
                throw new ArgumentException("Must not NULL or WHITESPACE", nameof(servicePrincipalName));

            var serviceApplication = await GetServiceApplicationInternalAsync(servicePrincipalName)
                .ConfigureAwait(false);

            if (serviceApplication is null)
                throw new ArgumentOutOfRangeException(nameof(servicePrincipalName));

            var servicePrincipal = await GetServicePrincipalAsync(servicePrincipalName)
                .ConfigureAwait(false);

            var token = await azureSessionService
                .AcquireTokenAsync(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(password))
                password = CreateServicePrincipalPassword();

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddYears(1);

            var patchPayloads = new List<object>()
            {
                new
                {
                    passwordCredentials = new[]
                    {
                        new
                        {
                            startDate,
                            endDate,
                            keyId = Guid.NewGuid(),
                            value = password,
                            customKeyIdentifier = Convert.ToBase64String(servicePrincipal.ObjectId.ToByteArray())
                        }
                    }
                },
                new
                {
                    requiredResourceAccess = new[]
                    {
                        new
                        {
                            resourceAppId = "00000003-0000-0000-c000-000000000000",
                            resourceAccess = new[]
                            {
                                new
                                {
                                    id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
                                    type = "Scope"
                                }
                            }
                        }
                    }
                }
            };

            var patchTasks = patchPayloads
                .Select(payload => $"https://graph.windows.net/{azureSessionService.Options.TenantId}/applications/{serviceApplication.Inner.ObjectId}"
                                    .SetQueryParam("api-version", "1.6")
                                    .WithOAuthBearerToken(token)
                                    .PatchJsonAsync(payload));

            await Task
                .WhenAll(patchTasks)
                .ConfigureAwait(false);

            servicePrincipal.Password = password;

            return servicePrincipal;
        }

        public async Task<AzureServicePrincipal> GetServicePrincipalAsync(string servicePrincipalName)
        {
            if (string.IsNullOrWhiteSpace(servicePrincipalName))
                throw new ArgumentException("Must not NULL or WHITESPACE", nameof(servicePrincipalName));

            var servicePrincipal = await GetServicePrincipalInternalAsync(servicePrincipalName)
                .ConfigureAwait(false);

            if (servicePrincipal is null)
                return null;

            var serviceApplication = await GetServiceApplicationInternalAsync(servicePrincipalName)
                .ConfigureAwait(false);

            if (serviceApplication is null)
                return null;

            var azureServicePrincipal = new AzureServicePrincipal()
            {
                ObjectId = Guid.Parse(servicePrincipal.Id),
                ApplicationId = Guid.Parse(servicePrincipal.ApplicationId),
                Name = servicePrincipal.Name
            };

            var token = await azureSessionService
                .AcquireTokenAsync(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var json = await $"https://graph.windows.net/{azureSessionService.Options.TenantId}/applications/{serviceApplication.Inner.ObjectId}"
                .SetQueryParam("api-version", "1.6")
                .WithOAuthBearerToken(token)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            var identifier = Convert.ToBase64String(azureServicePrincipal.ObjectId.ToByteArray());
            var expiresOn = json.SelectToken($"$.value[?(@.customKeyIdentifier == '{identifier}')].endDate")?.ToString();

            if (!string.IsNullOrEmpty(expiresOn) && DateTime.TryParse(expiresOn, out var expiresOnDateTime))
                azureServicePrincipal.ExpiresOn = expiresOnDateTime;

            return azureServicePrincipal;
        }

        private async Task<IServicePrincipal> GetServicePrincipalInternalAsync(string servicePrincipalName)
        {
            if (string.IsNullOrWhiteSpace(servicePrincipalName))
                throw new ArgumentException("Must not NULL or WHITESPACE", nameof(servicePrincipalName));

            servicePrincipalName = SanitizeServicePrincipalName(servicePrincipalName);

            try
            {
                return await azureSessionService.CreateSession()
                    .ServicePrincipals
                    .GetByNameAsync(servicePrincipalName)
                    .ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        private async Task<IActiveDirectoryApplication> GetServiceApplicationInternalAsync(string servicePrincipalName)
        {
            if (string.IsNullOrWhiteSpace(servicePrincipalName))
                throw new ArgumentException("Must not NULL or WHITESPACE", nameof(servicePrincipalName));

            servicePrincipalName = SanitizeServicePrincipalName(servicePrincipalName);

            try
            {
                return await azureSessionService.CreateSession()
                    .ActiveDirectoryApplications
                    .GetByNameAsync(servicePrincipalName)
                    .ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        public async Task DeleteServicePrincipalAsync(string servicePrincipalName)
        {
            if (string.IsNullOrWhiteSpace(servicePrincipalName))
                throw new ArgumentException("Must not NULL or WHITESPACE", nameof(servicePrincipalName));


            var session = azureSessionService.CreateSession();

            var tasks = new List<Task>()
            {
                GetServicePrincipalInternalAsync(servicePrincipalName)
                    .ContinueWith((task) => task.Result is null ? Task.CompletedTask : session.ServicePrincipals.DeleteByIdAsync(task.Result.Id), default, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current),

                GetServiceApplicationInternalAsync(servicePrincipalName)
                    .ContinueWith((task) => task.Result is null ? Task.CompletedTask : session.ActiveDirectoryApplications.DeleteByIdAsync(task.Result.Id), default, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current)
            };

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }
    }
}
