/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Diagnostics
{
    public interface IAdapterInitializationLoggerFactory
    {
        public Task<ILogger> CreateLoggerAsync(ComponentTask componentTask, ILogger logger);
    }
}
