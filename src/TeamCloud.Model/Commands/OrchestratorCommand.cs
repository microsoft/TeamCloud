/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorCommand
    {
        public ICommand Command { get; set; }

        public TeamCloudInstance TeamCloud { get; set; }

        public OrchestratorCommand() { }

        public OrchestratorCommand(TeamCloudInstance teamCloud, ICommand command)
        {
            TeamCloud = teamCloud;
            Command = command;
        }

        [JsonIgnore]
        public Guid CommandId => Command.CommandId;
    }
}
