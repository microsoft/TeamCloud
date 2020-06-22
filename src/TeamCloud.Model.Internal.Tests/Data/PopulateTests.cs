/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using TeamCloud.Model.Data.Core;
using Xunit;

namespace TeamCloud.Model.Internal.Data
{
    public class PopulateTests
    {
        static Dictionary<string, string> GetProperties(string type)
            => new Dictionary<string, string> {
                { $"{type}PropertyOne", $"{type}PropertyOneValue" },
                { $"{type}PropertyTwo", $"{type}PropertyTwoValue" }
            };

        static Dictionary<string, string> GetTags(string type)
            => new Dictionary<string, string> {
                { $"{type}TagOne", $"{type}TagOneValue" },
                { $"{type}TagTwo", $"{type}TagTwoValue" }
            };

        static User GetUser(string projectId)
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Role = TeamCloudUserRole.Admin,
                UserType = UserType.User,
                ProjectMemberships = new List<ProjectMembership> {
                    new ProjectMembership {
                        ProjectId = projectId,
                        Role = ProjectUserRole.Member,
                        Properties = GetProperties("ProjectMembership")
                    }
                },
                Properties = GetProperties("User")
            };
        }

        static AzureResourceGroup GetResourceGroup()
        {
            return new AzureResourceGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Region = "eastus",
                SubscriptionId = Guid.NewGuid(),
            };
        }

        static Provider GetProvider()
        {
            return new Provider
            {
                Id = "test.provider",
                AuthCode = Guid.NewGuid().ToString(),
                PrincipalId = Guid.NewGuid(),
                Registered = DateTime.UtcNow,
                Properties = GetProperties("Provider"),
                ResourceGroup = GetResourceGroup(),
                Url = "https://example.com"
            };
        }

        static ProjectType GetProjectType()
        {
            var provider = GetProvider();

            return new ProjectType
            {
                Id = "test.type",
                Default = true,
                Properties = GetProperties("ProjectType"),
                Providers = new List<ProviderReference> {
                    new ProviderReference {
                        Id = provider.Id,
                        Properties = provider.Properties
                    }
                },
                Region = "eastus",
                Subscriptions = new List<Guid> { Guid.NewGuid() },
                Tags = GetTags("ProjectType")
            };
        }

        static Project GetProject()
        {
            var projectId = Guid.NewGuid().ToString();

            return new Project
            {
                Id = projectId,
                Name = Guid.NewGuid().ToString(),
                Users = new List<User> {
                    GetUser(projectId),
                    GetUser(projectId)
                },
                Properties = GetProperties("Project"),
                ResourceGroup = GetResourceGroup(),
                Type = GetProjectType(),
                Tags = GetTags("Project")
            };
        }

        static Model.Data.User GetExternalUser(string projectId)
        {
            return new Model.Data.User
            {
                Id = Guid.NewGuid().ToString(),
                Role = TeamCloudUserRole.Admin,
                UserType = UserType.User,
                ProjectMemberships = new List<ProjectMembership> {
                    new ProjectMembership {
                        ProjectId = projectId,
                        Role = ProjectUserRole.Member,
                        Properties = GetProperties("ProjectMembership")
                    }
                },
                Properties = GetProperties("User")
            };
        }

        static Model.Data.Project GetExternalProject()
        {
            var projectId = Guid.NewGuid().ToString();

            return new Model.Data.Project
            {
                Id = projectId,
                Name = Guid.NewGuid().ToString(),
                Users = new List<Model.Data.User> {
                    GetExternalUser(projectId),
                    GetExternalUser(projectId)
                },
                Properties = GetProperties("Project"),
                ResourceGroup = GetResourceGroup(),
                // Type = GetProjectType(),
                Tags = GetTags("Project")
            };
        }


        [Fact]
        public void ProjectPopulateExternalModel()
        {
            var source = GetProject();
            var target = source.PopulateExternalModel();

            Assert.Equal(target.Id, source.Id);
            Assert.Equal(target.Name, source.Name);
            Assert.Equal(target.Properties, source.Properties);
            Assert.Equal(target.ResourceGroup, source.ResourceGroup);
            Assert.Equal(target.Tags, source.Tags);

            foreach (var user in source.Users)
            {
                Assert.Contains(new Model.Data.User
                {
                    Id = user.Id,
                    ProjectMemberships = user.ProjectMemberships,
                    Properties = user.Properties,
                    Role = user.Role,
                    UserType = user.UserType
                }, target.Users);
            }
        }

        [Fact]
        public void ProjectPopulateFromExternalModel()
        {
            var source = GetExternalProject();
            var target = new Project();
            target.PopulateFromExternalModel(source);

            Assert.Equal(target.Id, source.Id);
            Assert.Equal(target.Name, source.Name);
            Assert.Equal(target.Properties, source.Properties);
            Assert.Equal(target.ResourceGroup, source.ResourceGroup);
            Assert.Equal(target.Tags, source.Tags);

            foreach (var user in source.Users)
            {
                Assert.Contains(new User
                {
                    Id = user.Id,
                    ProjectMemberships = user.ProjectMemberships,
                    Properties = user.Properties,
                    Role = user.Role,
                    UserType = user.UserType
                }, target.Users);
            }
        }

        [Fact]
        public void UserPopulateExternalModel()
        {
            var projectId = Guid.NewGuid().ToString();
            var source = GetUser(projectId);
            var target = source.PopulateExternalModel();

            Assert.Equal(target.Id, source.Id);
            Assert.Equal(target.ProjectMemberships, source.ProjectMemberships);
            Assert.Equal(target.Properties, source.Properties);
            Assert.Equal(target.Role, source.Role);
            Assert.Equal(target.UserType, source.UserType);
        }

        [Fact]
        public void UserPopulateFromExternalModel()
        {
            var projectId = Guid.NewGuid().ToString();
            var source = GetExternalUser(projectId);
            var target = new User();
            target.PopulateFromExternalModel(source);

            Assert.Equal(target.Id, source.Id);
            Assert.Equal(target.ProjectMemberships, source.ProjectMemberships);
            Assert.Equal(target.Properties, source.Properties);
            Assert.Equal(target.Role, source.Role);
            Assert.Equal(target.UserType, source.UserType);
        }
    }
}
