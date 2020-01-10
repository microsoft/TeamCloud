/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using TeamCloud.Configuration.Options;
using TeamCloud.Model;

namespace TeamCloud.API
{
    public class UserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly Microsoft.Graph.GraphServiceClient graphServiceClient;

        public UserService(IHttpContextAccessor httpContextAccessor, AzureRMOptions azureRMOptions)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(azureRMOptions.ClientId)
                .WithTenantId(azureRMOptions.TenantId)
                .WithClientSecret(azureRMOptions.ClientSecret)
                .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            this.graphServiceClient = new Microsoft.Graph.GraphServiceClient(authProvider);
        }

        public Guid CurrentUserId()
        {
            // TODO: no clue if this works

            var objectId = httpContextAccessor.HttpContext.User.GetObjectId();

            return objectId;

            //return httpContextAccessor.HttpContext.User.GetObjectId();
        }

        private async Task<Guid?> GetUserId(string email)
        {
            try
            {
                // TODO this code only works with 'Azure Active Directory' users, not 'Microsoft Account' or 'External Azure Active Directory'
                var user = await this.graphServiceClient.Users[email]
                    .Request()
                    .GetAsync();

                Debug.WriteLine($"Successfully found Microsoft Graph user '{email}': {user.DisplayName}");
                return Guid.Parse(user.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errror: Could not find Microsoft Graph user with '{email}': {ex.ToString()}");
                return null;
            }
        }

        public async Task<User> GetUser(UserDefinition userDefinition)
        {
            var userId = await GetUserId(userDefinition.Email);

            if (!userId.HasValue) return null;

            return new User
            {
                Id = userId.Value, // TODO: Get the id using u.Email
                Role = userDefinition.Role, // TODO: validate
                Tags = userDefinition.Tags
            };
        }
    }
}
