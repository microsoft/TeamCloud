/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public abstract class Adapter : IAdapter
    {
        private static readonly JSchema dataSchemaEmpty = new JSchema() { Type = JSchemaType.Object };
        private static readonly JObject formSchemaEmpty = new JObject();

        private static string PrettyPrintDeploymentScopeType(DeploymentScopeType type)
            => Regex.Replace(Enum.GetName(typeof(DeploymentScopeType), type), @"\B[A-Z]", " $0");

#pragma warning disable CS0618 // Type or member is obsolete

        // IDistributedLockManager is marked as obsolete, because it's not ready for "prime time"
        // however; it is used to managed singleton function execution within the functions fx !!!

        private readonly IAuthorizationSessionClient sessionClient;
        private readonly IAuthorizationTokenClient tokenClient;
        private readonly IDistributedLockManager distributedLockManager;

        protected Adapter(IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient, IDistributedLockManager distributedLockManager)
        {
            this.sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
            this.tokenClient = tokenClient ?? throw new ArgumentNullException(nameof(tokenClient));
            this.distributedLockManager = distributedLockManager ?? throw new ArgumentNullException(nameof(distributedLockManager));
        }

#pragma warning restore CS0618 // Type or member is obsolete

        public abstract DeploymentScopeType Type { get; }

        public abstract IEnumerable<ComponentType> ComponentTypes { get; }

        public virtual string DisplayName
            => PrettyPrintDeploymentScopeType(Type);

        protected IAuthorizationSessionClient SessionClient
            => sessionClient;

        protected IAuthorizationTokenClient TokenClient
            => tokenClient;

        protected Task<AdapterLock> AcquireLockAsync(string lockId, params string[] lockIdQualifiers)
        {
            if (string.IsNullOrWhiteSpace(lockId))
                throw new ArgumentException($"'{nameof(lockId)}' cannot be null or whitespace.", nameof(lockId));

            return AcquireLockAsync(string.Join('/', lockIdQualifiers.Prepend(lockId).Where(item => !string.IsNullOrWhiteSpace(item))));
        }

        protected async Task<AdapterLock> AcquireLockAsync(string lockId, int leaseTimeoutInSeconds = 60, int acquisitionTimeoutInSeconds = 60)
        {
            if (string.IsNullOrWhiteSpace(lockId))
                throw new ArgumentException($"'{nameof(lockId)}' cannot be null or whitespace.", nameof(lockId));

            leaseTimeoutInSeconds = Math.Max(leaseTimeoutInSeconds, 0);
            acquisitionTimeoutInSeconds = Math.Max(acquisitionTimeoutInSeconds, 0);

            var distributedLock = await distributedLockManager
                .TryLockAsync(null, lockId, null, null, TimeSpan.FromSeconds(leaseTimeoutInSeconds), CancellationToken.None)
                .ConfigureAwait(false);

            var acquisitionTimeoutElapsed = 0;

            while (distributedLock is null && acquisitionTimeoutElapsed++ < acquisitionTimeoutInSeconds)
            {
                await Task
                    .Delay(1000)
                    .ConfigureAwait(false);

                distributedLock = await distributedLockManager
                    .TryLockAsync(null, lockId, null, null, TimeSpan.FromSeconds(leaseTimeoutInSeconds), CancellationToken.None)
                    .ConfigureAwait(false);
            }

            if (distributedLock is null)
                throw new TimeoutException($"Failed to acquire lock for id '{lockId}' within {acquisitionTimeoutInSeconds} sec.");

            return new AdapterLock(distributedLockManager, distributedLock);
        }

        public virtual Task<string> GetInputDataSchemaAsync()
            => Task.FromResult(dataSchemaEmpty.ToString(Formatting.None));

        public virtual Task<string> GetInputFormSchemaAsync()
            => Task.FromResult(formSchemaEmpty.ToString(Formatting.None));

        public virtual Task<NetworkCredential> GetServiceCredentialAsync(Component component)
            => Task.FromResult(default(NetworkCredential));

        public abstract Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);

        public abstract Task<Component> CreateComponentAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log);

        public abstract Task<Component> UpdateComponentAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log);

        public abstract Task<Component> DeleteComponentAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log);

    }
}
