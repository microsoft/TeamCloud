/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using Xunit;

namespace TeamCloud.Model
{
    public class CommandResultTests
    {
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
            var result = new MockCommandResult()
            {
                Result = new MockCommandEntity(),
            };

            var json = JsonConvert.SerializeObject(result);

            Assert.NotEmpty(json);
            Assert.Contains("$type", json);

            var resultObj = JsonConvert.DeserializeObject<ICommandResult>(json);

            Assert.IsType<MockCommandResult>(resultObj);
        }


        [Fact]
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
        public async Task Serialize_WithErrorsAsync()
        {
            var errors = new List<CommandError>
            {
                await CreateFlurlHttpExceptionAsync().ConfigureAwait(false),
                new NotSupportedException()
            };

            var result = new MockCommandResult()
            {
                Result = new MockCommandEntity(),
                Errors = errors
            };

            var json = JsonConvert.SerializeObject(result);

            Assert.NotEmpty(json);
        }

        [Fact]
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
        public async Task Deserialize_WithExceptionErrorsAsync()
        {
            var errors = new List<CommandError>
            {
                await CreateFlurlHttpExceptionAsync().ConfigureAwait(false),
                new NotSupportedException()
            };

            var result1 = new MockCommandResult()
            {
                Result = new MockCommandEntity(),
                Errors = errors
            };

            var json1 = JsonConvert.SerializeObject(result1);

            var result2 = JsonConvert.DeserializeObject<ICommandResult>(json1);

            var json2 = JsonConvert.SerializeObject(result2);

            Assert.Equal(json1, json2);
        }

        public class MockCommandEntity
        {

        }

        public class MockCommandResult : CommandResult<MockCommandEntity>
        {

        }
    }
}
