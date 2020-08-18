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
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.API.Data
{
    public class UpdateProviderDataTrigger
    {
        readonly IProviderDataRepository providerDataRepository;

        public UpdateProviderDataTrigger(IProviderDataRepository providerDataRepository)
        {
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }

        [FunctionName(nameof(UpdateProviderDataTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "data/providerData")] ProviderDataDocument providerData
            /* ILogger log */)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var newProviderData = await providerDataRepository
                .SetAsync(providerData)
                .ConfigureAwait(false);

            return new OkObjectResult(newProviderData);
        }
    }
}
