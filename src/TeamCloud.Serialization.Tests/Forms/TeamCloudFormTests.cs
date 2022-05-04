/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Serialization.Forms;
using Xunit;

namespace TeamCloud.Serialization.Tests.Forms;

public class TeamCloudFormTests
{
    [Fact]
    public async Task GetDataSchemaAsync()
    {
        var dataJson = await TeamCloudForm
            .GetDataSchemaAsync<SimpleFormData>()
            .ConfigureAwait(false);

        var dataRaw = dataJson.ToString();

        Assert.NotNull(dataJson);
    }

    [Fact]
    public async Task GetFormSchemaAsync()
    {
        var formJson = await TeamCloudForm
            .GetFormSchemaAsync<SimpleFormData>()
            .ConfigureAwait(false);

        var formRaw = formJson.ToString();

        Assert.NotNull(formJson);
    }

    [TeamCloudFormOrder(nameof(ValueTwo), nameof(ValueOne), "*")]
    public class SimpleFormData
    {
        public string ValueOne { get; set; }

        public string ValueTwo { get; set; }

        public bool ValueBool { get; set; }
    }
}
