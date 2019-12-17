/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TeamCloud
{
    public class UserAccessResult
    {
        public IUser User { get; set; }

        public bool HasAccess { get; set; }

        public string ErrorMessage { get; set; }

        public UnauthorizedObjectResult UnauthorizedResult => new UnauthorizedObjectResult(ErrorMessage);

        public UserAccessResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public UserAccessResult(IUser user, bool hasAccess, string errorMessage = null)
        {
            User = user;
            HasAccess = hasAccess;
            ErrorMessage = errorMessage;
        }
    }

    public static class UserExtensions
    {
        public static UserAccessResult ConfirmAccess(this HttpRequest request, TeamCloud teamCloud, TeamCloudUserRole minimumRole = TeamCloudUserRole.Admin)
        {
            var userId = request.ClientPrincipalId();

            if (string.IsNullOrEmpty(userId))
                return new UserAccessResult("You do not have access to this TeamCloud instance");
            
            var user = teamCloud.Users?.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return new UserAccessResult("You do not have access to this TeamCloud instance");

            if (user.Role < minimumRole)
                return new UserAccessResult(user, false, "You do not have sufficient permissions access resources from this TeamCloud instance");

            return new UserAccessResult(user, true);
        }

        public static UserAccessResult ConfirmAccess(this HttpRequest request, TeamCloud teamCloud, Project project, ProjectUserRole minimumRole)
        {
            var userId = request.ClientPrincipalId();

            if (string.IsNullOrEmpty(userId))
                return new UserAccessResult("You do not have access to this TeamCloud instance");

            var teamCloudUser = teamCloud.Users?.FirstOrDefault(u => u.Id == userId);
            var projectUser = project.Users?.FirstOrDefault(u => u.Id == userId);
            
            if (teamCloudUser == null && projectUser == null)
                return new UserAccessResult("You do not have permission to access this TeamCloud Project");

            if (projectUser != null && projectUser.Role >= minimumRole)
                return new UserAccessResult(projectUser, true);

            if (teamCloudUser != null && teamCloudUser.Role == TeamCloudUserRole.Admin)
                return new UserAccessResult(teamCloudUser, true);

            return new UserAccessResult(projectUser ?? teamCloudUser as IUser, false, "You do not have sufficient permissions access the requested resources from this TeamCloud Project");
        }
    }
}
