/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TeamCloud.Configuration.Options;

public static class Extensions
{
    public static IServiceCollection AddTeamCloudOptionsShared(this IServiceCollection services)
        => services.AddTeamCloudOptions(Assembly.GetExecutingAssembly());
}
