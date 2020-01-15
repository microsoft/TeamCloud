/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TeamCloud.Azure.Deployments
{
    [Serializable]
    public class AzureDeploymentException : Exception
    {
        public AzureDeploymentException()
            : base() { }

        public AzureDeploymentException(string message, string resourceId, string resourceError)
            : base(message)
        {
            ResourceId = resourceId;
            ResourceError = resourceError;
        }

        public AzureDeploymentException(string message, string resourceId, string resourceError, Exception inner)
            : base(message, inner)
        {
            ResourceId = resourceId;
            ResourceError = resourceError;
        }

        protected AzureDeploymentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ResourceId = info.GetString(nameof(ResourceId));
            ResourceError = info.GetString(nameof(ResourceError));
        }

        public string ResourceId { get; }
        public string ResourceError { get; }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ResourceId), ResourceId);
            info.AddValue(nameof(ResourceError), ResourceError);

            base.GetObjectData(info, context);
        }
    }
}
