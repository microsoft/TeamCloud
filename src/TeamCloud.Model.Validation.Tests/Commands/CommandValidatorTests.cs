/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;
using TeamCloud.Model.Commands;
using Xunit;

namespace TeamCloud.Model.Validation.Tests.Commands
{
    public class CommandValidatorTests
    {
        public CommandValidatorTests()
        {
            ReferenceLink.BaseUrl = "http://localhost/";
        }

        [Fact]
        public void Validate_Success()
        {
            var command = new OrchestratorProjectCreateCommand(new UserDocument(), new ProjectDocument());

            var result = command.Validate();

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_Success()
        {
            var command = new OrchestratorProjectCreateCommand(new UserDocument(), new ProjectDocument());

            var result = await command.ValidateAsync().ConfigureAwait(false);

            Assert.True(result.IsValid);
        }

        [Fact(Skip = "Needs rework as command throws exception if user argument is NULL")]
        public void Validate_Error()
        {
            var command = new OrchestratorProjectCreateCommand(null, new ProjectDocument());

            var result = command.Validate();

            Assert.False(result.IsValid);
        }

        [Fact(Skip = "Needs rework as command throws exception if user argument is NULL")]
        public async Task ValidateAsync_Error()
        {
            var command = new OrchestratorProjectCreateCommand(null, new ProjectDocument());

            var result = await command.ValidateAsync().ConfigureAwait(false);

            Assert.False(result.IsValid);
        }
    }
}
