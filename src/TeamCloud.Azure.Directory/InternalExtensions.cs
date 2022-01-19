/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Text;

namespace TeamCloud.Azure.Directory;

internal static class InternalExtensions
{
    internal static bool StartsWithHttp(this string value)
        => value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    internal static string EncodeBase64(this Encoding encoding, string text)
    {
        if (text is null)
            return null;

        byte[] textAsBytes = encoding.GetBytes(text);
        return Convert.ToBase64String(textAsBytes);
    }

    public static string DecodeBase64(this Encoding encoding, string encodedText)
    {
        if (encodedText is null)
            return null;

        byte[] textAsBytes = Convert.FromBase64String(encodedText);
        return encoding.GetString(textAsBytes);
    }
}
