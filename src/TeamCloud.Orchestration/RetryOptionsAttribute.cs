/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace TeamCloud.Orchestration
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class RetryOptionsAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<string, RetryOptionsAttribute> Cache = new ConcurrentDictionary<string, RetryOptionsAttribute>();

        internal static RetryOptionsAttribute GetByFunctionName(string functionName)
        {
            if (functionName is null)
                throw new ArgumentNullException(nameof(functionName));

            return Cache.GetOrAdd(functionName, (key) =>
            {
                var functionMethod = FunctionEnvironment.GetFunctionMethod(functionName);

                if (functionMethod is null)
                    throw new ArgumentOutOfRangeException(nameof(functionName), $"Could not find function by name '{functionName}'");

                return functionMethod.GetCustomAttribute<RetryOptionsAttribute>();
            });
        }

        /// <summary>
        /// Creates a new instance of <see cref="FunctionRetryAttribute"/>
        /// </summary>
        /// <param name="maxNumberOfAttempts">Sets the max number of attempts</param>
        public RetryOptionsAttribute(int maxNumberOfAttempts)
        {
            if (maxNumberOfAttempts < 1)
                throw new ArgumentException("Invalid number of attempts. Specify a value greater than zero.", nameof(maxNumberOfAttempts));

            MaxNumberOfAttempts = maxNumberOfAttempts;
        }

        private TimeSpan firstRetryInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets the first retry interval
        /// default to 1 minute
        /// </summary>
        public string FirstRetryInterval
        {
            get => firstRetryInterval.ToString();
            set => firstRetryInterval = TimeSpan.Parse(value);
        }

        private TimeSpan maxRetryInterval = TimeSpan.FromHours(1);

        /// <summary>
        /// Gets or sets the max retry interval
        /// defaults to 1 hour
        /// </summary>
        public string MaxRetryInterval
        {
            get => maxRetryInterval.ToString();
            set => maxRetryInterval = TimeSpan.Parse(value);
        }

        private TimeSpan retryTimeout = TimeSpan.FromDays(1);

        /// <summary>
        /// Gets or sets the timeout for retries
        /// defaults to 1 day
        /// </summary>
        public string RetryTimeout
        {
            get => retryTimeout.ToString();
            set => retryTimeout = TimeSpan.Parse(value);
        }

        /// <summary>
        /// Gets or sets the back-off coefficient
        /// defaults to 1, used to determine rate of increase of back-off
        /// </summary>
        public double BackoffCoefficient { get; set; } = 1;


        /// <summary>
        /// Gets or sets the max number of attempts
        /// </summary>
        public int MaxNumberOfAttempts { get; private set; }
    }
}
