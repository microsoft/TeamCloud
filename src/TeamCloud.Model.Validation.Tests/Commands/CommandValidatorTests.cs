/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
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

            var result = await command.ValidateAsync();

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_Error()
        {
            var command = new OrchestratorProjectCreateCommand(null, new Project());

            var result = command.Validate();

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_Error()
        {
            var command = new OrchestratorProjectCreateCommand(null, new Project());

            var result = await command.ValidateAsync();

            Assert.False(result.IsValid);
        }
    }
}
