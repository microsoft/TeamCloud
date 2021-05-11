/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;
using TeamCloud.Model.Handlers;

namespace TeamCloud.Adapters
{
    public abstract class Adapter : IAdapter
    {
        private readonly static ConcurrentDictionary<Type, Type[]> CommandHandlerTypes = new ConcurrentDictionary<Type, Type[]>();

        private readonly IServiceProvider serviceProvider;
        private readonly IAuthorizationSessionClient sessionClient;
        private readonly IAuthorizationTokenClient tokenClient;

        protected Adapter(IServiceProvider serviceProvider, IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
            this.tokenClient = tokenClient ?? throw new ArgumentNullException(nameof(tokenClient));
        }

        protected IAuthorizationSessionClient SessionClient
            => sessionClient;

        protected IAuthorizationTokenClient TokenClient
            => tokenClient;

        public virtual IEnumerable<ICommandHandler> GetCommandHandlers()
        {
            var commandHandlerTypes = CommandHandlerTypes.GetOrAdd(GetType(), type => type.Assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(t))
                .ToArray());

            foreach (var commandHandlerType in commandHandlerTypes)
                yield return (ICommandHandler)ActivatorUtilities.CreateInstance(serviceProvider, commandHandlerType, new object[] { this });
        }

        public abstract Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);

        public abstract bool Supports(DeploymentScope deploymentScope);
    }
}
