/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TeamCloud.Data;

namespace TeamCloud.Orchestrator.API.Data
{
    public class DeleteProjectTypeTrigger
    {
        readonly IProjectTypesRepository projectTypesRepository;

        public DeleteProjectTypeTrigger(IProjectTypesRepository projectTypesRepository)
        {
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        [FunctionName(nameof(DeleteProjectTypeTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "data/projectTypes/{projectTypeId}")] HttpRequest httpRequest,
            string projectTypeId
            /* ILogger log */)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (projectTypeId is null)
                throw new ArgumentNullException(nameof(projectTypeId));

            var projectType = await projectTypesRepository
                .GetAsync(projectTypeId)
                .ConfigureAwait(false);

            if (projectType is null)
                return new NotFoundResult();

            await projectTypesRepository
                .RemoveAsync(projectType)
                .ConfigureAwait(false);

            return new OkObjectResult(projectType);
        }
    }
}
