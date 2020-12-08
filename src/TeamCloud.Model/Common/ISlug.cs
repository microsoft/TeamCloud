/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Diagnostics.CodeAnalysis;
using Slugify;

namespace TeamCloud.Model.Common
{
    public interface ISlug : IDisplayName
    {
        [SuppressMessage("Naming", "CA1721: Property names should not match get methods", Justification = "Workaround for default interface property")]
        string Slug { get; }

        public string GetSlug()
            => new SlugHelper().GenerateSlug(DisplayName ?? string.Empty);
    }
}
