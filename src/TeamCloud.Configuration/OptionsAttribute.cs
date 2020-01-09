/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Configuration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class OptionsAttribute : Attribute
    {
        public OptionsAttribute(string sectionName = null) 
            => SectionName = sectionName;

        public string SectionName { get; private set; }
    }
}
