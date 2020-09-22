/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;

namespace TeamCloud.API.Middleware
{
    public sealed class RequestResponseTracingMiddleware : IMiddleware
    {
        private readonly ILogger logger;
        private readonly RecyclableMemoryStreamManager streamManager;
        private readonly ObjectPool<StringBuilder> stringBuilderPool;

        public RequestResponseTracingMiddleware(ILogger<RequestResponseTracingMiddleware> logger, RecyclableMemoryStreamManager streamManager, ObjectPool<StringBuilder> stringBuilderPool)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.streamManager = streamManager ?? throw new ArgumentNullException(nameof(streamManager));
            this.stringBuilderPool = stringBuilderPool ?? throw new ArgumentNullException(nameof(stringBuilderPool));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (next is null)
                throw new ArgumentNullException(nameof(next));

            if (logger.IsEnabled(LogLevel.Debug))
            {
                try
                {
                    using var tempBodyStream = streamManager.GetStream();

                    var stopwatch = Stopwatch.StartNew();
                    var originalBodyStream = context.Response.Body;

                    await LogRequest(context)
                        .ConfigureAwait(false);

                    context.Response.Body = tempBodyStream;

                    await next(context)
                        .ConfigureAwait(false);

                    await LogResponse(context, stopwatch)
                        .ConfigureAwait(false);

                    if (tempBodyStream.CanRead)
                    {
                        tempBodyStream.Seek(0, SeekOrigin.Begin);

                        await tempBodyStream
                            .CopyToAsync(originalBodyStream)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception exc)
                {
                    logger.LogWarning(exc, $"Failed to trace request/response: {exc.Message}");

                    throw;
                }
            }
            else
            {
                await next(context)
                    .ConfigureAwait(false);
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            using var requestStream = streamManager.GetStream();

            try
            {
                context.Request.EnableBuffering();

                await context.Request.Body
                    .CopyToAsync(requestStream)
                    .ConfigureAwait(false);
            }
            finally
            {
                context.Request.Body.Position = 0;
            }

            var message = stringBuilderPool.Get();

            try
            {
                message.AppendLine($"Http Request Information: {context.Request.Method.ToUpperInvariant()} {context.Request.GetDisplayUrl()}");
                message.AppendLine($"User:  {context.User?.GetObjectId()}");
                message.AppendLine($"Roles: {string.Join(", ", GetUserRoles())}");
                message.AppendLine($"Body:  {await ReadStreamAsync(requestStream).ConfigureAwait(false)}");

                logger.LogDebug(message.ToString());
            }
            finally
            {
                stringBuilderPool.Return(message);
            }

            IEnumerable<string> GetUserRoles()
                => (context.User?.Claims ?? Enumerable.Empty<Claim>())
                .Where(claim => ClaimTypes.Role.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
                .Select(claim => claim.Value);
        }

        private async Task LogResponse(HttpContext context, Stopwatch stopwatch)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var message = stringBuilderPool.Get();

            try
            {
                message.AppendLine($"Http Response Information: {context.Request.Method.ToUpperInvariant()} {context.Request.GetDisplayUrl()} [{stopwatch.Elapsed}]");
                message.AppendLine($"Body: {await ReadStreamAsync(context.Response.Body).ConfigureAwait(false)}");

                logger.LogDebug(message.ToString());
            }
            finally
            {
                stringBuilderPool.Return(message);
            }
        }

        private static async Task<string> ReadStreamAsync(Stream stream)
        {
            string content = null;

            if (stream?.CanRead ?? false)
            {
                var position = stream.Position;

                try
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    using var reader = new StreamReader(
                        stream,
                        Encoding.Default,
                        true,
                        1024,
                        leaveOpen: true);

                    content = await reader
                        .ReadToEndAsync()
                        .ConfigureAwait(false);
                }
                finally
                {
                    stream.Position = position;
                }
            }

            return content;
        }
    }
}
