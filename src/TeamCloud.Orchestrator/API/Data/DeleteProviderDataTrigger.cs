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
    public class DeleteProviderDataTrigger
    {
        readonly IProviderDataRepository providerDataRepository;

        public DeleteProviderDataTrigger(IProviderDataRepository providerDataRepository)
        {
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }

        [FunctionName(nameof(DeleteProviderDataTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "data/providerData/{providerDataId}")] HttpRequest httpRequest,
            string providerDataId
            /* ILogger log */)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (providerDataId is null)
                throw new ArgumentNullException(nameof(providerDataId));

            var providerData = await providerDataRepository
                .GetAsync(providerDataId)
                .ConfigureAwait(false);

            if (providerData is null)
                return new NotFoundResult();

            await providerDataRepository
                .RemoveAsync(providerData)
                .ConfigureAwait(false);

            return new OkObjectResult(providerData);
        }
    }
}
