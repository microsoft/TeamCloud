/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Adapters
{
    public interface IAdapterConfiguration
    {
        IAdapterConfiguration Register<TAdapter>()
            where TAdapter: class, IAdapter;
    }
}
