/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace TeamCloud.Azure.Directory
{
    internal static class InternalExtensions
    {
        internal static bool IsGuid(this string value)
            => Guid.TryParse(value, out var _);

        internal static bool IsEMail(this string value)
            => new EmailAddressAttribute().IsValid(value);
    }
}
