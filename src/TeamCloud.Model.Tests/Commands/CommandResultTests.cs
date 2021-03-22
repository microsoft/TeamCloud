/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using Xunit;

namespace TeamCloud.Model
{
    public class CommandResultTests
    {
        private readonly User user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            Organization = Guid.NewGuid().ToString()
        };

        private async Task<FlurlHttpException> CreateFlurlHttpExceptionAsync()
        {
            var flurlHttpException = default(FlurlHttpException);

            try
            {
                _ = await $"http://{Guid.NewGuid()}.com"
                    .GetAsync()
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException exc)
            {
                flurlHttpException = exc;
            }

            return flurlHttpException;
        }

        [Fact]
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
        public void Serialize()
        {
            var command = new MockCommand(this.user, MockPayload.Instance);
            var result = command.CreateResult();

            var json = TeamCloudSerialize.SerializeObject(result);

            Assert.NotEmpty(json);
            Assert.Contains("$type", json);

            var resultObj = TeamCloudSerialize.DeserializeObject<ICommandResult>(json);

            Assert.IsType<MockCommandResult>(resultObj);
        }


        [Fact]
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
        public async Task Serialize_WithErrorsAsync()
        {
            var command = new MockCommand(this.user, MockPayload.Instance);

            var result1 = command.CreateResult();

            result1.Errors = new List<CommandError>
            {
                await CreateFlurlHttpExceptionAsync().ConfigureAwait(false),
                new NotSupportedException()
            };

            var json1 = TeamCloudSerialize.SerializeObject(result1);

            Assert.NotEmpty(json1);
            Assert.Contains("$type", json1);

            var result2 = TeamCloudSerialize.DeserializeObject<ICommandResult>(json1);

            var json2 = TeamCloudSerialize.SerializeObject(result2);

            Assert.NotEmpty(json2);
            Assert.Contains("$type", json2);

            Assert.Equal(json1, json2);
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
