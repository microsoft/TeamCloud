/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.Adapters;
using TeamCloud.API.Auth;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("adapters")]
    [Produces("application/json")]
    public class AdapterController : TeamCloudController
    {
        private readonly IEnumerable<IAdapter> adapters;

        public AdapterController(IEnumerable<IAdapter> adapters = null) : base()
        {
            this.adapters = adapters ?? Enumerable.Empty<IAdapter>();
        }

        [HttpGet()]
        [Authorize(Policy = AuthPolicies.Default)]
        [SwaggerOperation(OperationId = "GetAdapters", Summary = "Gets all Adapters.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Adapters.", typeof(DataResult<List<AdapterInformation>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var adpaterInformationList = await adapters
                .Select(async a => new AdapterInformation()
                {
                    Type = a.Type,
                    DisplayName = a.DisplayName,
                    InputDataSchema = await a.GetInputDataSchemaAsync().ConfigureAwait(false),
                    InputDataForm = await a.GetInputFormSchemaAsync().ConfigureAwait(false)
                })
                .ToAsyncEnumerable()
                .OrderBy(ai => ai.Type != DeploymentScopeType.AzureResourceManager).ThenBy(ai => ai.DisplayName)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<AdapterInformation>>
                .Ok(adpaterInformationList)
                .ToActionResult();
        }
    }
}
