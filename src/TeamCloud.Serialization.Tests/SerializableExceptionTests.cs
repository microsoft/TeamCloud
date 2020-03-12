/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using Xunit;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration.Tests
{
    public class SerializableExceptionTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void DeSerialize_Exception(int nestingLevel)
        {
            try
            {
                ThrowException_Exception(nestingLevel);
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
            {
                AssertException(serializableException);
            }
            catch (Exception exc)
            {
                AssertException(exc);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public async Task DeSerialize_FlurlHttpException(int nestingLevel)
        {
            try
            {
                await ThrowException_FlurlHttpExceptionError(nestingLevel)
                    .ConfigureAwait(false);
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
            {
                AssertException(serializableException);
            }
            catch (Exception exc)
            {
                AssertException(exc);
            }
        }

        [Fact]
        public async Task DeSerialize_FlurlHttpExceptionRaw()
        {
            try
            {
                await ThrowException_FlurlHttpExceptionError()
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                AssertException(exc, false);
            }
        }

        private void ThrowException_Exception(int nestingLevel = 0)
        {
            if (nestingLevel <= 0)
            {
                throw new Exception();
            }
            else
            {
                try
                {
                    ThrowException_Exception(nestingLevel - 1);
                }
                catch (Exception exc)
                {
                    var baseExc = exc.GetBaseException();

                    throw new Exception($"{baseExc.Message} (Level {nestingLevel})", exc);
                }
            }
        }

        private async Task ThrowException_FlurlHttpExceptionError(int nestingLevel = 0)
        {
            if (nestingLevel <= 0)
            {
                await $"http://{Guid.NewGuid()}"
                    .GetAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                try
                {
                    await ThrowException_FlurlHttpExceptionError(nestingLevel - 1)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    var baseExc = exc.GetBaseException();

                    throw new Exception($"{baseExc.Message} (Level {nestingLevel})", exc);
                }
            }
        }

        private void AssertException(Exception serializableException, bool serializableExpected = true)
        {
            Assert.Equal(serializableExpected, serializableException.IsSerializable(out _));

            string serializableExceptionJson;

            try
            {
                serializableExceptionJson = JsonConvert.SerializeObject(serializableException, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }
            catch
            {
                serializableExceptionJson = default;
            }

            if (serializableExpected)
            {
                Assert.NotNull(serializableExceptionJson);
            }

            if (!string.IsNullOrEmpty(serializableExceptionJson))
            {
                Exception deserializedException;

                try
                {
                    deserializedException = (Exception)JsonConvert.DeserializeObject(serializableExceptionJson, serializableException.GetType());
                }
                catch when (!serializableExpected)
                {
                    deserializedException = default;
                }

                if (serializableExpected)
                {
                    Assert.NotNull(deserializedException);
                }
            }
        }

    }
}
