/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Diagnostics.CodeAnalysis;

namespace TeamCloud.Model.Common
{
    public interface ISlug : IDisplayName
    {
        [SuppressMessage("Naming", "CA1721: Property names should not match get methods", Justification = "Workaround for default interface property")]
        string Slug { get; }

        public string GetSlug() => DisplayName?
            .ToLowerInvariant()
            .Replace(' ', '-');
    }
}
