/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using Xunit;

namespace TeamCloud.Model.Commands
{
    public class CommandTests
    {
        private readonly User user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            Organization = Guid.NewGuid().ToString()
        };

        [Fact]
        public void Serialize()
        {
            var command = new MockCommand(this.user, MockPayload.Instance);

            var json = TeamCloudSerialize.SerializeObject(command);

            Assert.NotEmpty(json);
            Assert.Contains("$type", json);

            var resultObj = TeamCloudSerialize.DeserializeObject<ICommand>(json);

            Assert.IsType<MockCommand>(resultObj);
        }

        public class MockPayload
        {
            public static readonly MockPayload Instance = new MockPayload();
        }

        public class MockCommand : CustomCommand<MockPayload, MockCommandResult>
        {
            public MockCommand(User user, MockPayload payload) : base(user, payload)
            { }
        }

        public class MockCommandResult : CommandResult<MockPayload>
        {
        }
    }
}
