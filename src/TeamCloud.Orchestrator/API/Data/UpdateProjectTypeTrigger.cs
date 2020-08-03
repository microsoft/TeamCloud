/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Orchestrator.API.Data
{
    public class UpdateProjectTypeTrigger
    {
        readonly IProjectTypesRepository projectTypesRepository;

        public UpdateProjectTypeTrigger(IProjectTypesRepository projectTypesRepository)
        {
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        [FunctionName(nameof(UpdateProjectTypeTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "data/projectTypes")] ProjectTypeDocument projectType
            /* ILogger log */)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            var newProjectType = await projectTypesRepository
                .SetAsync(projectType)
                .ConfigureAwait(false);

            return new OkObjectResult(newProjectType);
        }
    }
}
