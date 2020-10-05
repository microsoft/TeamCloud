/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Services;

namespace TeamCloud.API.Controllers
{
    public abstract class ApiController : ControllerBase
    {
        protected ApiController(UserService userService, Orchestrator orchestrator)
        {
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            Orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        public string ProjectId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

        public string ProviderId
            => RouteData.Values.GetValueOrDefault(nameof(ProviderId), StringComparison.OrdinalIgnoreCase)?.ToString();

        public UserService UserService { get; }

        public Orchestrator Orchestrator { get; }
    }
}
