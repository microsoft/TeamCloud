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
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public class UserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IMemoryCache cache;

        public UserService(IHttpContextAccessor httpContextAccessor, IAzureDirectoryService azureDirectoryService, IMemoryCache cache)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.cache = cache;
        }

        public Guid CurrentUserId
            => httpContextAccessor.HttpContext.User.GetObjectId();

        private async Task<Guid?> GetUserIdAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));

            Guid? val = null;

            // Generate unique key for this identifier
            string key = $"{nameof(UserService)}_{nameof(GetUserIdAsync)}_{identifier}";

            // See if the cache has this key value
            if (!cache.TryGetValue<Guid?>(key, out val))
            {
                // Key doesn't exist, query for UserID
                val = await azureDirectoryService.GetUserIdAsync(identifier).ConfigureAwait(false);

                // Set value to cache so long as it's a valid Guid
                if(val.HasValue && val.Value != Guid.Empty)
                    cache.Set(key, val, TimeSpan.FromMinutes(5)); // Cached value only for certain amount of time
            }
            
            return val;
        }

        public async Task<User> GetUserAsync(UserDefinition userDefinition)
        {
            var userId = await GetUserIdAsync(userDefinition.Email)
                .ConfigureAwait(false);

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
