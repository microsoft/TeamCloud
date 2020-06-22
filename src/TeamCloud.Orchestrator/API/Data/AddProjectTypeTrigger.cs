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
    public class AddProjectTypeTrigger
    {
        readonly IProjectTypesRepository projectTypesRepository;

        public AddProjectTypeTrigger(IProjectTypesRepository projectTypesRepository)
        {
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        [FunctionName(nameof(AddProjectTypeTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "data/projectTypes")] ProjectType projectType
            /* ILogger log */)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            var newProjectType = await projectTypesRepository
                .AddAsync(projectType)
                .ConfigureAwait(false);

            return new OkObjectResult(newProjectType);
        }
    }
}
