/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
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
        public async Task Serialize_WithErrorsAsync()
        {
            var flurlHttpException = await CreateFlurlHttpExceptionAsync().ConfigureAwait(false);
            var notSupportedException = new NotSupportedException();

            var result = new MockCommandResult()
            {
                Result = new MockCommandEntity(),
                Exceptions = new List<Exception>()
                {
                    flurlHttpException,
                    notSupportedException
                }
            };

            var json = JsonConvert.SerializeObject(result);

            Assert.NotEmpty(json);
        }

        [Fact]
        public async Task Deserialize_WithExceptionErrorsAsync()
        {
            var flurlHttpException = await CreateFlurlHttpExceptionAsync().ConfigureAwait(false);
            var notSupportedException = new NotSupportedException();

            var result1 = new MockCommandResult()
            {
                Result = new MockCommandEntity(),
                Exceptions = new List<Exception>()
                {
                    flurlHttpException,
                    notSupportedException
                }
            };

            var json1 = JsonConvert.SerializeObject(result1);

            var result2 = JsonConvert.DeserializeObject<ICommandResult>(json1);

            var json2 = JsonConvert.SerializeObject(result2);

            Assert.Equal(json1, json2);
        }

        class MockCommandEntity
        {

        }

        class MockCommandResult : CommandResult<MockCommandEntity>
        {

        }
    }
}
