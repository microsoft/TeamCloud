/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace TeamCloud.Data.Conditional
{
    public class ConditionalFactAttribute : FactAttribute
    {
        private static ConditionalFactPlatforms GetCurrentPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                return ConditionalFactPlatforms.FreeBSD;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return ConditionalFactPlatforms.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return ConditionalFactPlatforms.OSX;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ConditionalFactPlatforms.Windows;

            throw new NotSupportedException("Unsupported OS platform");
        }

        public ConditionalFactAttribute(ConditionalFactPlatforms platforms)
        {
            var currentPlatform = GetCurrentPlatform();

            if ((platforms & currentPlatform) != currentPlatform)
                Skip ??= $"Fact is not supported on '{currentPlatform}'";
        }
    }
}
