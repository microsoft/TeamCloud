/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;

namespace TeamCloud.API.Initialization;

public interface IHostInitializer
{
    Task InitializeAsync();
}
