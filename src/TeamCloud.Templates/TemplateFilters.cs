/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Globalization;

namespace TeamCloud.Templates
{
    public static class TemplateFilters
    {
        public static string Format(object input, string format)
        {
            if (input == null)
                return null;
            else if (string.IsNullOrWhiteSpace(format))
                return input.ToString();

            return string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", input);
        }
    }
}
