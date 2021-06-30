/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Serialization.Forms;
using Xunit;

namespace TeamCloud.Serialization.Tests.Forms
{
    public class TeamCloudFormTests
    {
        [Fact]
        public async Task GetFormJsonAsync()
        {
            var formJson = await TeamCloudForm
                .GetFormSchemaAsync<SimpleFormData>()
                .ConfigureAwait(false);

            Assert.NotNull(formJson);
        }

        [TeamCloudFormOrder(nameof(ValueTwo), nameof(ValueOne))]
        public class SimpleFormData
        {
            public string ValueOne { get; set; }

            public string ValueTwo { get; set; }
        }
    }
}
