/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public sealed class ProjectCommandNotification
    {
        public string InstanceId { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
