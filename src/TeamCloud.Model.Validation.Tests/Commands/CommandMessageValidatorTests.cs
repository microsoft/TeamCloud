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
    public class CommandMessageValidatorTests
    {
        [Fact]
        public void Validate_Success()
        {
            var command = new ProjectCreateCommand(new User(), new Project());
            var message = new ProviderCommandMessage(command, "http://localhost/callback");

            var result = message.Validate();

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_Success()
        {
            var command = new ProjectCreateCommand(new User(), new Project());
            var message = new ProviderCommandMessage(command, "http://localhost/callback");

            var result = await message.ValidateAsync();

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_Error()
        {
            var command = new ProjectCreateCommand(null, new Project());
            var message = new ProviderCommandMessage(command, "http://localhost/callback");

            var result = message.Validate();

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_Error()
        {
            var command = new ProjectCreateCommand(null, new Project());
            var message = new ProviderCommandMessage(command, "http://localhost/callback");

            var result = await message.ValidateAsync();

            Assert.False(result.IsValid);
        }
    }
}
