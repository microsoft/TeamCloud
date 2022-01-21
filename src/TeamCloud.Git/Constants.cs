/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Text.RegularExpressions;

namespace TeamCloud.Git;

public static class Constants
{
    public const string Readme = "readme.md";
    public const string ProjectYaml = "project.yaml";
    public const string ComponentYaml = "component.yaml";

    public static string TagRef(string version) => $"refs/tags/{version}";
    public static string BranchRef(string version) => $"refs/heads/{version}";

    // 2 seconds should be enough mostly, per DataAnnotations class - http://index/?query=REGEX_DEFAULT_MATCH_TIMEOUT
    public static readonly Regex ValidSha1 = new Regex(@"\b[0-9a-f]{40}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromSeconds(2));

}
