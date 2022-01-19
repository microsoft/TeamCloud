/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;
using TeamCloud.Model.Commands;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Tests.Commands;

public class CommandValidatorTests
{
    private readonly IValidatorProvider validatorProvider;

    public CommandValidatorTests()
    {
        var serviceCollection = new ServiceCollection().AddTeamCloudValidationProvider(config =>
        {
            config.Register(Assembly.GetExecutingAssembly());
        });

        validatorProvider = serviceCollection.BuildServiceProvider().GetService<IValidatorProvider>();
    }

    [Fact]
    public void Validate_Success()
    {
        var command = new ProjectCreateCommand(new User(), new Project());

        var result = command.Validate(validatorProvider);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_Success()
    {
        var command = new ProjectCreateCommand(new User(), new Project());

        var result = await command.ValidateAsync(validatorProvider).ConfigureAwait(false);

        Assert.True(result.IsValid);
    }

    [Fact(Skip = "Needs rework as command throws exception if user argument is NULL")]
    public void Validate_Error()
    {
        var command = new ProjectCreateCommand(null, new Project());

        var result = command.Validate(validatorProvider);

        Assert.False(result.IsValid);
    }

    [Fact(Skip = "Needs rework as command throws exception if user argument is NULL")]
    public async Task ValidateAsync_Error()
    {
        var command = new ProjectCreateCommand(null, new Project());

        var result = await command.ValidateAsync(validatorProvider).ConfigureAwait(false);

        Assert.False(result.IsValid);
    }
}
