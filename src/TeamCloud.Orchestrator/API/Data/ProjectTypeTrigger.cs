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
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.API.Data
{
    public sealed class ProjectTypeTrigger
    {
        readonly IProjectTypeRepository projectTypesRepository;

        public ProjectTypeTrigger(IProjectTypeRepository projectTypesRepository)
        {
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        [FunctionName(nameof(ProjectTypeTrigger) + nameof(Post))]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "data/projectTypes")] ProjectTypeDocument projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            var newProjectType = await projectTypesRepository
                .AddAsync(projectType)
                .ConfigureAwait(false);

            return new OkObjectResult(newProjectType);
        }

        [FunctionName(nameof(ProjectTypeTrigger) + nameof(Put))]
        public async Task<IActionResult> Put(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "data/projectTypes")] ProjectTypeDocument projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            var newProjectType = await projectTypesRepository
                .SetAsync(projectType)
                .ConfigureAwait(false);

            return new OkObjectResult(newProjectType);
        }

        [FunctionName(nameof(ProjectTypeTrigger) + nameof(Delete))]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "data/projectTypes/{projectTypeId}")] HttpRequest httpRequest, string projectTypeId)
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
