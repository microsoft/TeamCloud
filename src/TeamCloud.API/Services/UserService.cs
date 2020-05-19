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
using TeamCloud.Model;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public class UserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IMemoryCache cache;
        readonly IUsersRepository usersRepository;

        public UserService(IHttpContextAccessor httpContextAccessor, IAzureDirectoryService azureDirectoryService, IMemoryCache cache, IUsersRepository usersRepository)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        public Guid CurrentUserId
            => httpContextAccessor.HttpContext.User.GetObjectId();

        public async Task<User> CurrentUserAsync()
        {
            var user = await usersRepository
                .GetAsync(CurrentUserId)
                .ConfigureAwait(false);

            return user;
        }

        public async Task<Guid?> GetUserIdAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));

            // handle passing in the id as a string
            if (Guid.TryParse(identifier, out var userId))
                return userId;

            string key = $"{nameof(UserService)}_{nameof(GetUserIdAsync)}_{identifier}";

            if (!cache.TryGetValue(key, out Guid? val))
            {
                val = await azureDirectoryService.GetUserIdAsync(identifier).ConfigureAwait(false);

                if (val.HasValue && val.Value != Guid.Empty)
                    cache.Set(key, val, TimeSpan.FromMinutes(5)); // Cached value only for certain amount of time
            }

            return val;
        }

        public async Task<User> ResolveUserAsync(UserDefinition userDefinition)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var userId = await GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (!userId.HasValue || userId.Value == Guid.Empty)
                return null;

            var user = await usersRepository
                .GetAsync(userId.Value)
                .ConfigureAwait(false);

            user ??= new User
            {
                Id = userId.Value,
                UserType = UserType.User
            };

            return user;
        }
    }
}
