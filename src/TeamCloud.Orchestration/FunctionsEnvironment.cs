/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;

namespace TeamCloud.Orchestration;

public static class FunctionsEnvironment
{
    private static readonly ConcurrentDictionary<string, MethodInfo> FunctionMethodCache = new();

    public static MethodInfo GetFunctionMethod(string functionName) => FunctionMethodCache.GetOrAdd(functionName, (key) =>
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .SelectMany(asm => asm.GetExportedTypes().Where(type => type.IsClass))
            .SelectMany(type => type.GetMethods())
            .FirstOrDefault(method => method.GetCustomAttribute<FunctionNameAttribute>()?.Name.Equals(functionName, StringComparison.Ordinal) ?? false);

    }) ?? throw new ArgumentOutOfRangeException(nameof(functionName), $"Could not find function by name '{functionName}'");
}
