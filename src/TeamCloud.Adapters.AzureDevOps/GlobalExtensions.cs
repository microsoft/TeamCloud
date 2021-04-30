/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace TeamCloud.Adapters.AzureDevOps
{
    public static class GlobalExtensions
    {
        internal static string UrlDecode(this string source)
            => HttpUtility.UrlDecode(source ?? string.Empty);

        internal static Dictionary<string, string[]> ToDictionary(this NameValueCollection collection)
            => collection.Cast<string>().ToDictionary(key => key, key => collection.GetValues(key));

        internal static bool TryGetValue(this NameValueCollection collection, string key, out string value)
        {
            value = collection.AllKeys.Contains(key)
                ? collection.Get(key) : default;

            return value != default;
        }

    }
}
