using System;
using Newtonsoft.Json;
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
                Role = Data.Core.TeamCloudUserRole.Admin,
                UserType = Data.Core.UserType.User
            };

            var command = new ProviderRegisterCommand(user, new ProviderConfiguration()) { ProviderId = "foo.provider" };
            var commandJson = JsonConvert.SerializeObject(command);

            var command2 = JsonConvert.DeserializeObject<ProviderRegisterCommand>(commandJson);
            var commandJson2 = JsonConvert.SerializeObject(command2);

            Assert.Equal(commandJson, commandJson2);
        }
    }
}
