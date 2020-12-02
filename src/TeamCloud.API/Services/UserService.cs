/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.API.Data;
using TeamCloud.Azure.Directory;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public class UserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IMemoryCache cache;
        readonly IUserRepository userRepository;

        public UserService(IHttpContextAccessor httpContextAccessor, IAzureDirectoryService azureDirectoryService, IMemoryCache cache, IUserRepository userRepository)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public string CurrentUserId
            => httpContextAccessor.HttpContext.User.GetObjectId();

        public string CurrentUserTenant
            => httpContextAccessor.HttpContext.User.GetTenantId();

        /// <summary>
        ///
        /// </summary>
        /// <param name="allowUnsafe">This should only be set to true in ApiController actions with the attribute Authorize(Policy = AuthPolicies.Default)</param>
        /// <returns></returns>
        public async Task<User> CurrentUserAsync(string organizationId, bool allowUnsafe = false)
        {
            User user = null;

            if (!string.IsNullOrEmpty(organizationId))
            {
                user = await userRepository
                    .GetAsync(organizationId, CurrentUserId)
                    .ConfigureAwait(false);
            }

            if (user is null && allowUnsafe)
            {
                user = new User
                {
                    Id = CurrentUserId,
                    Organization = organizationId,
                    Role = OrganizationUserRole.None,
                    UserType = UserType.User
                };
            }

            return user;
        }

        public async Task<string> GetUserIdAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));

            string key = $"{nameof(UserService)}_{nameof(GetUserIdAsync)}_{identifier}";

            if (!cache.TryGetValue(key, out string val))
            {
                var guid = await azureDirectoryService
                    .GetUserIdAsync(identifier)
                    .ConfigureAwait(false);

                val = guid?.ToString();

                if (!string.IsNullOrEmpty(val))
                    cache.Set(key, val, TimeSpan.FromMinutes(5));
            }

            return val;
        }

        public async Task<User> ResolveUserAsync(string organizationId, UserDefinition userDefinition, UserType userType = UserType.User)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var userId = await GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await userRepository
                .GetAsync(organizationId, userId)
                .ConfigureAwait(false);

            user ??= new User
            {
                Id = userId,
                UserType = userType,
                Organization = organizationId
            };

            return user;
        }
    }
}
