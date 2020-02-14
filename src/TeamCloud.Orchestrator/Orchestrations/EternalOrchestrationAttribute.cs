/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Orchestrator.Orchestrations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class EternalOrchestrationAttribute : Attribute
    {
        public EternalOrchestrationAttribute(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException($"The parameter {nameof(instanceId)} must contain a unique string that identifies the eternal orchestration instance", nameof(instanceId));

            InstanceId = instanceId;
        }

        public string InstanceId { get; }

        public string OrchestrationName { get; set; }
    }
}
