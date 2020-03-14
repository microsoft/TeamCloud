/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Deployment
{
    [Serializable]
    public class AzureDeploymentException : Exception
    {
        internal static IEnumerable<string> ResolveResourceErrors(JToken errorToken)
            => ResolveResourceErrorTokens(errorToken).Select(token => $"[{token.SelectToken("code")}] {token.SelectToken("message")}");

        internal static IEnumerable<JToken> ResolveResourceErrorTokens(JToken errorToken)
        {
            if (errorToken is null)
                return Enumerable.Empty<JToken>();

            var detailTokens = errorToken.SelectTokens("details[*]").ToList();

            if (detailTokens.Any())
                return detailTokens.SelectMany(detailToken => ResolveResourceErrorTokens(detailToken));

            var message = errorToken.SelectToken("message")?.ToString();

            if ((message?.StartsWith('{') ?? false) && (message?.EndsWith('}') ?? false))
                return ResolveResourceErrorTokens(JObject.Parse(message).SelectToken("error"));

            return Enumerable.Repeat(errorToken, 1);
        }

        public AzureDeploymentException()
            : base() { }

        public AzureDeploymentException(string message, string resourceId, string[] resourceErrors)
            : base(message)
        {
            ResourceId = resourceId;
            ResourceErrors = resourceErrors ?? Array.Empty<string>();
        }

        public AzureDeploymentException(string message, string resourceId, string[] resourceErrors, Exception inner)
            : base(message, inner)
        {
            ResourceId = resourceId;
            ResourceErrors = resourceErrors ?? Array.Empty<string>();
        }

        protected AzureDeploymentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ResourceId = info.GetString(nameof(ResourceId));
            ResourceErrors = info.GetString(nameof(ResourceErrors))?.Split('|') ?? Array.Empty<string>();
        }

        public string ResourceId { get; }

        public string[] ResourceErrors { get; } = Array.Empty<string>();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ResourceId), ResourceId);
            info.AddValue(nameof(ResourceErrors), string.Join("|", ResourceErrors));
        }

        public override string ToString()
        {
            var message = base.ToString();

            if (ResourceErrors.Any())
                message += $" ---> Azure Resource Manager{Environment.NewLine}{string.Join(Environment.NewLine, ResourceErrors)}";

            return message;
        }
    }
}
