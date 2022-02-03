/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TeamCloud.Http;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Services;

public sealed class OrchestratorService
{
    private readonly IOrchestratorServiceOptions options;
    private readonly IValidatorProvider validatorProvider;
    private readonly IHttpContextAccessor httpContextAccessor;

    public OrchestratorService(IOrchestratorServiceOptions options, IValidatorProvider validatorProvider, IHttpContextAccessor httpContextAccessor)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.validatorProvider = validatorProvider ?? throw new ArgumentNullException(nameof(validatorProvider));
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public Task<ICommandResult> QueryAsync(ICommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        return QueryAsync(command.CommandId, command.ProjectId);
    }

    public async Task<ICommandResult> QueryAsync(Guid commandId, string projectId)
    {
        var commandResponse = await options.Url
            .AppendPathSegment($"api/command/{commandId}")
            .WithHeader("x-functions-key", options.AuthCode)
            .AllowHttpStatus(HttpStatusCode.NotFound)
            .UseTeamCloudSerializer()
            .GetAsync()
            .ConfigureAwait(false);

        if (commandResponse.StatusCode == StatusCodes.Status404NotFound)
            return null;

        var commandResult = await commandResponse
            .GetJsonAsync<ICommandResult>()
            .ConfigureAwait(false);

        commandResult.SetResultLinks(httpContextAccessor, commandResponse.ResponseMessage, projectId);

        return commandResult;
    }

    public async Task<ICommandResult> InvokeAsync(ICommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command is IValidatable validatable)
        {
            _ = await validatable
                .ValidateAsync(validatorProvider, throwOnValidationError: true)
                .ConfigureAwait(false);
        }

        var commandResult = command.CreateResult();

        try
        {
            var commandResponse = await options.Url
                .AppendPathSegment("api/command")
                .WithHeader("x-functions-key", options.AuthCode)
                .UseTeamCloudSerializer()
                .PostJsonAsync(command)
                .ConfigureAwait(false);

            commandResult = await commandResponse
                .GetJsonAsync<ICommandResult>()
                .ConfigureAwait(false);

            commandResult.SetResultLinks(httpContextAccessor, commandResponse.ResponseMessage, command.ProjectId);
        }
        catch (FlurlHttpTimeoutException timeoutExc)
        {
            commandResult ??= command.CreateResult();
            commandResult.Errors.Add(timeoutExc);
        }
        catch (FlurlHttpException serviceUnavailableExc) when (serviceUnavailableExc.StatusCode == StatusCodes.Status503ServiceUnavailable)
        {
            commandResult ??= command.CreateResult();
            commandResult.Errors.Add(serviceUnavailableExc);
        }

        return commandResult;
    }
}
