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
    public class AddProviderDataTrigger
    {
        readonly IProviderDataRepository providerDataRepository;

        public AddProviderDataTrigger(IProviderDataRepository providerDataRepository)
        {
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }

        [FunctionName(nameof(AddProviderDataTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "data/providerData")] ProviderDataDocument providerData
            /* ILogger log */)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var newProviderData = await providerDataRepository
                .AddAsync(providerData)
                .ConfigureAwait(false);

            return new OkObjectResult(newProviderData);
        }
    }
}
