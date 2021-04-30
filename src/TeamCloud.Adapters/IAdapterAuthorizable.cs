/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Adapters
{
    public interface IAdapterAuthorizable : IAdapter
    {
        public AuthorizationSession CreateAuthorizationSession(IContainerDocument containerDocument)
        {
            if (containerDocument is null)
                throw new ArgumentNullException(nameof(containerDocument));

            var type = typeof(IAdapterAuthorizable<>)
                .MakeGenericType(containerDocument.GetType());

            if (type?.IsAssignableFrom(this.GetType()) ?? false)
            {
                return (AuthorizationSession)type
                    .GetMethod(nameof(CreateAuthorizationSession), new Type[] { containerDocument.GetType() })
                    .Invoke(this, new[] { containerDocument });
            }

            throw new NotSupportedException($"Adapter context of type {containerDocument.GetType()} is not supported.");
        }

        Task<IActionResult> HandleAuthorizeAsync(HttpRequestMessage requestMessage, AuthorizationSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log);

        Task<IActionResult> HandleCallbackAsync(HttpRequestMessage requestMessage, AuthorizationSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log);
    }

    public interface IAdapterAuthorizable<TContext> : IAdapterAuthorizable, IAdapter<TContext>
        where TContext : class, IContainerDocument, new()
    {
        AuthorizationSession CreateAuthorizationSession(TContext containerDocument);
    }
}
