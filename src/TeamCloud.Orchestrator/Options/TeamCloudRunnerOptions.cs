/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;

namespace TeamCloud.Orchestrator.Options;

public interface IRunnerOptions
{
    bool ImportDockerHub { get; }

    string WebServerImage { get; }
}

[Options("Runner")]
internal class TeamCloudRunnerOptions : IRunnerOptions
{
    public bool ImportDockerHub { get; set; } = false;

    public string WebServerImage { get; set; } = "teamcloud.azurecr.io/teamcloud/tcsidecar-webserver";
}
