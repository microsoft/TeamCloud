using System;
using System.Collections.Generic;
using TeamCloud.Model.Data;
using Xunit;

namespace TeamCloud.Model.Validation.Tests.Data
{
    public class TeamCloudConfigurationValidatorTests
    {
        [Fact]
        public void Validate_Success()
        {
            var configuration = new TeamCloudConfiguration()
            {
                ProjectTypes = new List<ProjectType>()
                {
                    new ProjectType()
                    {
                         Id = "default",
                         Region = "WestUS",
                         Subscriptions = new List<Guid>()
                         {
                             Guid.NewGuid()
                         },
                         ResourceGroupNamePrefix = "tc_",
                         Providers = new List<ProjectTypeProvider>()
                         {
                             new ProjectTypeProvider()
                             {
                                 Id = "providerA"
                             },
                             new ProjectTypeProvider()
                             {
                                 Id = "providerB"
                             }
                         }
                    }
                }
            };

            var result = configuration.Validate();

            Assert.True(result.IsValid);
        }
    }
}
