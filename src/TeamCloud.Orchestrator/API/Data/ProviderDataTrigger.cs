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
    public sealed class ProviderDataTrigger
    {
        readonly IProviderDataRepository providerDataRepository;

        public ProviderDataTrigger(IProviderDataRepository providerDataRepository)
        {
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }

        [FunctionName(nameof(ProviderDataTrigger) + nameof(Post))]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "data/providerData")] ProviderDataDocument providerData)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var newProviderData = await providerDataRepository
                .AddAsync(providerData)
                .ConfigureAwait(false);

            return new OkObjectResult(newProviderData);
        }

        [FunctionName(nameof(ProviderDataTrigger) + nameof(Put))]
        public async Task<IActionResult> Put(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "data/providerData")] ProviderDataDocument providerData)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var newProviderData = await providerDataRepository
                .SetAsync(providerData)
                .ConfigureAwait(false);

            return new OkObjectResult(newProviderData);
        }

        [FunctionName(nameof(ProviderDataTrigger) + nameof(Delete))]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "data/providerData/{providerDataId}")] HttpRequest httpRequest,
            string providerDataId)
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
