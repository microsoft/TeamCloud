/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Reflection;

namespace TeamCloud.Orchestration
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class FunctionsImportAttribute : Attribute
    {
        public FunctionsImportAttribute(Type assemblyReferenceType)
        {
            if (assemblyReferenceType is null)
                throw new ArgumentNullException(nameof(assemblyReferenceType));

            Assembly = assemblyReferenceType.Assembly;
        }

        public Assembly Assembly { get; private set; }
    }
}
