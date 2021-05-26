/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public abstract class Adapter : IAdapter
    {
        private static readonly JSchema dataSchemaEmpty = new JSchema() { Type = JSchemaType.Object };
        private static readonly JObject formSchemaEmpty = new JObject();

        private static string PrettyPrintDeploymentScopeType(DeploymentScopeType type)
            => Regex.Replace(Enum.GetName(typeof(DeploymentScopeType), type), @"\B[A-Z]", " $0");

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

        public virtual Task<NetworkCredential> GetServiceCredentialAsync(Component component)
            => Task.FromResult(default(NetworkCredential));

        public abstract Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);
        public abstract Task<Component> CreateComponentAsync(Component component);
        public abstract Task<Component> UpdateComponentAsync(Component component);
        public abstract Task<Component> DeleteComponentAsync(Component component);
    }
}
