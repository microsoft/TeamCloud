/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using Xunit;

namespace TeamCloud.Model.Commands
{
    public class ProviderRegisterCommandTests
    {
        public ProviderRegisterCommandTests()
        {
            ReferenceLink.BaseUrl = "http://localhost";
        }

        [Fact]
        public void SerializeDeserialize()
        {
            Assert.NotNull(ReferenceLink.BaseUrl);

            var user = new Data.User()
            {
                Id = Guid.Empty.ToString(),
                Role = TeamCloudUserRole.Admin,
                UserType = UserType.User
            };

            var command = new ProviderRegisterCommand(user, new ProviderConfiguration()) { ProviderId = "foo.provider" };
            var commandJson = JsonConvert.SerializeObject(command);

            var command2 = JsonConvert.DeserializeObject<ProviderRegisterCommand>(commandJson);
            var commandJson2 = JsonConvert.SerializeObject(command2);

            Assert.Equal(commandJson, commandJson2);
        }
    }
}
