/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Slugify;

namespace TeamCloud.Model.Common
{
    public interface ISlug : IDisplayName
    {
        public static string CreateSlug(IDisplayName instance)
            => instance is null
            ? throw new ArgumentNullException(nameof(instance))
            : new SlugHelper().GenerateSlug(instance.DisplayName ?? string.Empty);

        string Slug { get; }
    }
}
