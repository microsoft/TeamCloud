/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;
using TeamCloud.Model.Handlers;

namespace TeamCloud.Adapters
{
    public abstract class Adapter : IAdapter
    {
        private static readonly JSchema dataSchemaEmpty = new JSchema() { Type = JSchemaType.Object };
        private static readonly JObject formSchemaEmpty = new JObject();

        private static string PrettyPrintDeploymentScopeType(DeploymentScopeType type)
            => Regex.Replace(Enum.GetName(typeof(DeploymentScopeType), type), @"\B[A-Z]", " $0");

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

        public abstract DeploymentScopeType Type { get; }

        public virtual string DisplayName
            => PrettyPrintDeploymentScopeType(Type);

        protected IAuthorizationSessionClient SessionClient
            => sessionClient;

        protected IAuthorizationTokenClient TokenClient
            => tokenClient;

        public virtual Task<string> GetInputDataSchemaAsync()
            => Task.FromResult(dataSchemaEmpty.ToString(Formatting.None));

        public virtual Task<string> GetInputFormSchemaAsync()
            => Task.FromResult(formSchemaEmpty.ToString(Formatting.None));

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
    }
}
