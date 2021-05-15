/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Handlers;

namespace TeamCloud.Adapters
{
    public abstract class AdapterCommandHandler : CommandHandler
    {
        public AdapterCommandHandler(IAdapter adapter, bool orchestration = false) : base(orchestration)
        { }
    }
}
