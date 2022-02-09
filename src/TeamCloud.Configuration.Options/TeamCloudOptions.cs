/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Configuration.Options;

public interface ITeamCloudOptions
{
    string Version { get; }
}


[Options("TeamCloud")]
public sealed class TeamCloudOptions : ITeamCloudOptions
{
    public string Version { get; set; }
}
