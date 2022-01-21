/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TeamCloud.Serialization;
using Xunit;

namespace TeamCloud.Orchestration.Tests;

public class SerializableExceptionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private void AssertException(Exception serializableException, bool serializableExpected = true)
    {
        Assert.Equal(serializableExpected, serializableException.IsSerializable(out _));

        string serializableExceptionJson;

        try
        {
            serializableExceptionJson = TeamCloudSerialize.SerializeObject(serializableException, Formatting.Indented);
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
                deserializedException = (Exception)TeamCloudSerialize.DeserializeObject(serializableExceptionJson, serializableException.GetType());
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
