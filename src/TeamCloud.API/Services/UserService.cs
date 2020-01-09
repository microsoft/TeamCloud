/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TeamCloud.Model;

namespace TeamCloud.API
{
    public class UserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
            // TODO: call graph to get id from email

            return Guid.NewGuid();
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
