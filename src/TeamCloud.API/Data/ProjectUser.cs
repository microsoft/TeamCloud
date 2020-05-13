/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ProjectUser
    {
        public Guid Id { get; set; }

        public ProjectUserRole Role { get; set; }

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public ProjectUser() { }

        public ProjectUser(User user, Guid projectId)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var membership = user.ProjectMembership(projectId);

            if (membership is null)
                throw new ArgumentException("User must have a matching project membership", nameof(projectId));

            Id = user.Id;
            Role = membership.Role;
            Properties = membership.Properties;
        }
    }

    public sealed class ProjectUserValidator : AbstractValidator<ProjectUser>
    {
        public ProjectUserValidator()
        {
            RuleFor(obj => obj.Id).NotEqual(Guid.Empty)
                .WithMessage("'{PropertyName}' must be a valid, non-empty GUID.");
            //RuleFor(obj => obj.Role).MustBeUserRole();
        }
    }

    public static class ProjectUserExtensions
    {
        public static bool IsOwner(this ProjectUser user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.Role == ProjectUserRole.Owner;
        }
    }

}
