/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Globalization;

namespace TeamCloud.Templates;

public static class TemplateFilters
{
    public static string Format(object input, string format, string culture = null)
    {
        if (input == null)
            return null;
        else if (string.IsNullOrWhiteSpace(format))
            return input.ToString();

        var cultureInfo = string.IsNullOrWhiteSpace(culture)
            ? CultureInfo.CurrentCulture
            : CultureInfo.GetCultureInfo(culture);

        return string.Format(cultureInfo, "{0:" + format + "}", input);
    }
}
