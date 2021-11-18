using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using k8s;
using k8s.KubeConfigModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Directory;
using TeamCloud.Data;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Secrets;
using TeamCloud.Serialization;
using TeamCloud.Serialization.Forms;
using User = TeamCloud.Model.Data.User;
using KubernetesClient = k8s.Kubernetes;
using k8s.Models;
using Prometheus;
using Microsoft.Rest;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using Microsoft.OData.Edm;
using TeamCloud.Http;
using Flurl.Http;
using System.Net;
using YamlDotNet.Serialization;

namespace TeamCloud.Adapters.Kubernetes
{
    public sealed class KubernetesAdapter : Adapter
    {
        private readonly IAzureResourceService azureResourceService;

        public KubernetesAdapter(IAuthorizationSessionClient sessionClient,
                                 IAuthorizationTokenClient tokenClient,
                                 IDistributedLockManager distributedLockManager,
                                 ISecretsStoreProvider secretsStoreProvider,
                                 IAzureSessionService azureSessionService,
                                 IAzureResourceService azureResourceService,
                                 IAzureDirectoryService azureDirectoryService,
                                 IOrganizationRepository organizationRepository,
                                 IDeploymentScopeRepository deploymentScopeRepository,
                                 IProjectRepository projectRepository,
                                 IUserRepository userRepository) 
            : base(sessionClient, tokenClient, distributedLockManager, secretsStoreProvider, azureSessionService, azureDirectoryService, organizationRepository, deploymentScopeRepository, projectRepository, userRepository)
        {
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        public override DeploymentScopeType Type 
            => DeploymentScopeType.Kubernetes;

        public override IEnumerable<ComponentType> ComponentTypes 
            => new ComponentType[] { ComponentType.Namespace };

        public override Task<string> GetInputDataSchemaAsync()
            => TeamCloudForm.GetDataSchemaAsync<KubernetesData>()
            .ContinueWith(t => t.Result.ToString(), TaskContinuationOptions.OnlyOnRanToCompletion);

        public override Task<string> GetInputFormSchemaAsync()
            => TeamCloudForm.GetFormSchemaAsync<KubernetesData>()
            .ContinueWith(t => t.Result.ToString(), TaskContinuationOptions.OnlyOnRanToCompletion);

        private KubernetesData GetKubernetesData(DeploymentScope deploymentScope)
            => TeamCloudSerialize.DeserializeObject<KubernetesData>(deploymentScope.InputData);

        private IKubernetes GetKubernetesClient(KubernetesData kubernetesData)
        {
            if (kubernetesData is null)
                throw new ArgumentNullException(nameof(kubernetesData));

            var kubernetesConfig = Yaml.LoadFromString<K8SConfiguration>(kubernetesData.Yaml);

            return GetKubernetesClient(kubernetesConfig);
        }

        private IKubernetes GetKubernetesClient(K8SConfiguration kubernetesConfiguration)
        {
            if (kubernetesConfiguration is null)
                throw new ArgumentNullException(nameof(kubernetesConfiguration));

            var clientConfiguration = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubernetesConfiguration);

            return new KubernetesClient(clientConfiguration);
            //return new KubernetesClient(clientConfiguration, FlurlHttp.GlobalSettings.HttpClientFactory.CreateHttpClient(), false);

        }

        private async Task<T> WithKubernetesContext<T>(Component component, DeploymentScope deploymentScope, Func<IKubernetes, KubernetesData, V1ClusterRole, V1ServiceAccount, Task<T>> callback)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var identity = await azureResourceService
                .GetResourceAsync<AzureIdentityResource>(component.IdentityId, throwIfNotExists: true)
                .ConfigureAwait(false);

            var data = GetKubernetesData(deploymentScope);
            var client = GetKubernetesClient(data);

            var roleDefinition = new V1ClusterRole()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "teamcloud-runner"
                },
                Rules = new List<V1PolicyRule>()
                {
                    new V1PolicyRule()
                    {
                        ApiGroups = new List<string>() { "", "extensions", "apps" },
                        Resources = new List<string>() { "*" },
                        Verbs = new List<string>() { "*" }
                    },
                    new V1PolicyRule()
                    {
                        ApiGroups = new List<string>() { "batch" },
                        Resources = new List<string>() { "jobs", "cronjobs" },
                        Verbs = new List<string>() { "*" }
                    }
                }
            };

            try
            {
                roleDefinition = await client
                    .CreateClusterRoleAsync(roleDefinition)
                    .ConfigureAwait(false);
            }
            catch (HttpOperationException exc) when (exc.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                roleDefinition = await client
                    .ReadClusterRoleAsync(roleDefinition.Metadata.Name)
                    .ConfigureAwait(false);
            }

            var serviceAccount = new V1ServiceAccount()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = $"{data.Namespace}-{identity.PrincipalId}"
                }
            };

            try
            {
                serviceAccount = await client
                        .CreateNamespacedServiceAccountAsync(serviceAccount, data.Namespace)
                        .ConfigureAwait(false);
            }
            catch (HttpOperationException exc) when (exc.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                serviceAccount = await client
                    .ReadNamespacedServiceAccountAsync(serviceAccount.Metadata.Name, data.Namespace)
                    .ConfigureAwait(false);
            }

            return await callback(client, data, roleDefinition, serviceAccount).ConfigureAwait(false);
        }

        protected override Task<Component> CreateComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue)
            => WithKubernetesContext(component, deploymentScope, async (client, data, roleDefinition, serviceAccount) =>
            {
                var componentNamespace = new V1Namespace()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = $"{data.Namespace}-{component.Id}"
                    }
                };

                try
                {
                    componentNamespace = await client
                        .CreateNamespaceAsync(componentNamespace)
                        .ConfigureAwait(false);
                }
                catch (HttpOperationException exc) when (exc.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    componentNamespace = await client
                        .ReadNamespaceAsync(componentNamespace.Metadata.Name)
                        .ConfigureAwait(false);
                }

                var roleBinding = new V1RoleBinding()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "runner"
                    },
                    RoleRef = new V1RoleRef()
                    {
                        ApiGroup = roleDefinition.ApiGroup(),
                        Kind = roleDefinition.Kind,
                        Name = roleDefinition.Name()
                    },
                    Subjects = new List<V1Subject>()
                    {
                        new V1Subject()
                        {
                            ApiGroup = serviceAccount.ApiGroup(),
                            Kind = serviceAccount.Kind,
                            Name = serviceAccount.Name(),
                            NamespaceProperty = serviceAccount.Namespace()
                        }
                    }
                };

                try
                {
                    await client
                        .CreateNamespacedRoleBindingAsync(roleBinding, componentNamespace.Metadata.Name)
                        .ConfigureAwait(false);
                }
                catch (HttpOperationException exc) when (exc.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    await client
                        .ReplaceNamespacedRoleBindingAsync(roleBinding, roleBinding.Metadata.Name, componentNamespace.Metadata.Name)
                        .ConfigureAwait(false);
                }

                return component;
            });

        protected override Task<Component> DeleteComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue)
            => WithKubernetesContext(component, deploymentScope, async (client, data, roleDefinition, serviceAccount) =>
            {
                try
                {
                    await client
                        .DeleteNamespaceAsync($"{data.Namespace}-{component.Id}")
                        .ConfigureAwait(false);
                }
                catch (HttpOperationException exc) when (exc.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // swallow - the namespace was already deleted
                }

                return component;
            });

        protected override Task<Component> UpdateComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue)
            => WithKubernetesContext(component, deploymentScope, (client, data, roleDefinition, serviceAccount) =>
            {
                //TODO: implement some update logic - e.g. permission management for project users

                return Task.FromResult(component);
            });

        protected override Task<NetworkCredential> GetServiceCredentialAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project)
            => WithKubernetesContext(component, deploymentScope, async (client, data, roleDefinition, serviceAccount) =>
            {
                var configuration = await client
                    .CreateClusterConfigAsync(serviceAccount)
                    .ConfigureAwait(false);

                return new NetworkCredential()
                {
                    Domain = client.BaseUri.ToString(),
                    Password = new SerializerBuilder().Build().Serialize(configuration),
                    UserName = serviceAccount.Name()
                };
            });
    }
}
