/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TeamCloud.API.Data;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services
{
    public class UserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAzureDirectoryService azureDirectoryService;

        public UserService(IHttpContextAccessor httpContextAccessor, IAzureDirectoryService azureDirectoryService)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        }

        public Guid CurrentUserId
            => httpContextAccessor.HttpContext.User.GetObjectId();

        private Task<Guid?> GetUserIdAsync(string email)
        {
            return azureDirectoryService.GetUserIdAsync(email);
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
