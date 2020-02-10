/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TeamCloud.Azure.Directory;

namespace TeamCloud.Azure
{
    public static class Extensions
    {
        public static IAzureConfiguration AddDirectory(this IAzureConfiguration azureConfiguration)
        {
            azureConfiguration.Services
                .TryAddSingleton<IAzureDirectoryService, AzureDirectoryService>();

            return azureConfiguration;
        }


        internal static bool IsGuid(this string value)
            => Guid.TryParse(value, out var _);

        internal static bool IsEMail(this string value)
            => new EmailAddressAttribute().IsValid(value);


    }
}
