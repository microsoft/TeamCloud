/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Internal.Commands;
using TeamCloud.Model.Internal.Data;
using Xunit;

namespace TeamCloud.Model.Validation.Tests.Commands
{
    public class CommandValidatorTests
    {
        [Fact]
        public void Validate_Success()
        {
            var command = new OrchestratorProjectCreateCommand(new User(), new Project());

            var result = command.Validate();

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_Success()
        {
            var command = new OrchestratorProjectCreateCommand(new User(), new Project());

            var result = await command.ValidateAsync().ConfigureAwait(false);

            Assert.True(result.IsValid);
        }

        [Fact(Skip = "Needs rework as command throws exception if user argument is NULL")]
        public void Validate_Error()
        {
            var command = new OrchestratorProjectCreateCommand(null, new Project());

            var result = command.Validate();

            Assert.False(result.IsValid);
        }

        [Fact(Skip = "Needs rework as command throws exception if user argument is NULL")]
        public async Task ValidateAsync_Error()
        {
            var command = new OrchestratorProjectCreateCommand(null, new Project());

            var result = await command.ValidateAsync().ConfigureAwait(false);

            Assert.False(result.IsValid);
        }
    }
}
