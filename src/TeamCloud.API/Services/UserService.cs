/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.API.Data;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Services;

public class UserService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IGraphService graphService;
    private readonly IMemoryCache cache;
    readonly IUserRepository userRepository;

    public UserService(IHttpContextAccessor httpContextAccessor, IGraphService graphService, IMemoryCache cache, IUserRepository userRepository)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
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
    public async Task<User> CurrentUserAsync(string organizationId, string organizationName, bool allowUnsafe = false)
    {
        User user = null;

        if (!string.IsNullOrEmpty(CurrentUserId))
        {
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
                    Organization = organizationId ?? Guid.Empty.ToString(),
                    OrganizationName = organizationName ?? "none",
                    Role = OrganizationUserRole.None,
                    UserType = UserType.User
                };
            }
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
            var guid = await graphService
                .GetUserIdAsync(identifier)
                .ConfigureAwait(false);

            val = guid?.ToString();

            if (!string.IsNullOrEmpty(val))
                cache.Set(key, val, TimeSpan.FromMinutes(5));
        }

        return val;
    }

    public async Task<User> ResolveUserAsync(string organizationId, string organizationName, UserDefinition userDefinition, UserType userType = UserType.User)
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
            Organization = organizationId,
            OrganizationName = organizationName
        };

        return user;
    }
}
