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
using TeamCloud.Model.Internal.Data;

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

        public string CurrentUserId
            => httpContextAccessor.HttpContext.User.GetObjectId();

        public async Task<UserDocument> CurrentUserAsync()
        {
            var user = await usersRepository
                .GetAsync(CurrentUserId)
                .ConfigureAwait(false);

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

        public async Task<UserDocument> ResolveUserAsync(UserDefinition userDefinition, UserType userType = UserType.User)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var userId = await GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await usersRepository
                .GetAsync(userId)
                .ConfigureAwait(false);

            user ??= new UserDocument
            {
                Id = userId,
                UserType = userType
            };

            return user;
        }
    }
}
