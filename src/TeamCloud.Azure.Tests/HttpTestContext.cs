/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Flurl.Http;
using Flurl.Http.Testing;

namespace TeamCloud.Azure.Tests
{
    public abstract class HttpTestContext : IDisposable
    {
        private bool disposed = false;

        private readonly HttpTest testContext = new HttpTest();

        protected IEnumerable<HttpCall> CallLog
            => testContext.CallLog;

        protected IDisposable WithResponses(string sequenceName)
        {
            foreach (var response in GetResponseMessages(sequenceName))
                testContext.ResponseQueue.Enqueue(response);

            return new ResponseScope(testContext.ResponseQueue);
        }

        private IEnumerable<HttpResponseMessage> GetResponseMessages(string sequenceName)
        {
            var expression = new Regex($"{this.GetType().FullName}.{sequenceName}.(\\d+).response");

            var matches = this.GetType().Assembly.GetManifestResourceNames()
                .Select(name => expression.Match(name))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups[1].Value));

            var responses = matches.Select(match => GetResponseMessage(match.Value));

            return new Queue<HttpResponseMessage>(responses);
        }

        private HttpResponseMessage GetResponseMessage(string resourceName)
        {
            using (var reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream(resourceName)))
            {
                var line = reader.ReadLine();

                while (!line.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase))
                    line = reader.ReadLine(); // skip comment line

                var status = line.Split(' ')[1];
                var statusCode = (HttpStatusCode)(int)float.Parse(status);

                var response = new HttpResponseMessage(statusCode);

                while (true)
                {
                    var header = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(header)) break;

                    var tokens = header.Split(new char[] { ':' }, 2);
                    response.SetHeader(tokens[0], tokens[1].TrimStart());
                }

                var buffer = Encoding.UTF8.GetBytes(reader.ReadToEnd());

                response.SetHeader("Content-Length", buffer.Length);
                response.Content = new ByteArrayContent(buffer);

                return response;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                testContext.Dispose();

            disposed = true;
        }

        private class ResponseScope : IDisposable
        {
            private readonly Queue<HttpResponseMessage> queue;

            public ResponseScope(Queue<HttpResponseMessage> queue)
            {
                this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            }

            public void Dispose() => queue.Clear();
        }
    }
}
