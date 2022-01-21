/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Reflection;

namespace TeamCloud.Validation.Providers;

public interface IValidatorProviderConfig
{
    public IValidatorProviderConfig Register(Assembly assembly);
}
