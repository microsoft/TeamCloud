/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using DurableTask.Core.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;

namespace TeamCloud.Orchestration
{
    public interface IRetryOptionsFactory
    {
        RetryOptions GetRetryOptions(string functionName, Func<Exception, bool> handle = null);
    }

    public class RetryOptionsFactory : IRetryOptionsFactory
    {
        internal static readonly IRetryOptionsFactory Default = new RetryOptionsFactory();

        private readonly IConfiguration configuration;

        public RetryOptionsFactory(IConfiguration configuration = null)
        {
            this.configuration = configuration;
        }

        public virtual RetryOptions GetRetryOptions(string functionName, Func<Exception, bool> handle = null)
        {
            var retryAttribute = RetryOptionsAttribute.GetByFunctionName(functionName) ?? new RetryOptionsAttribute(1);

            var retryOptions = new RetryOptions(TimeSpan.Parse(retryAttribute.FirstRetryInterval), retryAttribute.MaxNumberOfAttempts)
            {
                MaxRetryInterval = TimeSpan.Parse(retryAttribute.MaxRetryInterval),
                RetryTimeout = TimeSpan.Parse(retryAttribute.RetryTimeout),
                BackoffCoefficient = retryAttribute.BackoffCoefficient,
                Handle = (exc) => HandleException(exc is TaskFailedException taskFailedExc ? taskFailedExc.InnerException : exc)
            };

            configuration?
                .GetSection($"Orchestration:RetryOptions:{functionName}")
                .Bind(retryOptions);

            return retryOptions;

            bool HandleException(Exception exc)
            {
                Debug.WriteLine($"Function '{functionName}': Execution failed -> {exc}");

                return handle?.Invoke(exc) ?? retryAttribute.RetryHandler?.Handle(exc) ?? true;
            }
        }
    }
}
