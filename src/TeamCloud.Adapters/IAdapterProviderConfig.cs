/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Adapters
{
    public interface IAdapterProviderConfig
    {
        IAdapterProviderConfig Register<TAdapter>()
            where TAdapter: class, IAdapter;
    }
}
