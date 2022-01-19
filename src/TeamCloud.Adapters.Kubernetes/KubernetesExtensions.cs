/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using System.Text.RegularExpressions;
using System.Linq;
using k8s.KubeConfigModels;
using k8s.Models;
using k8s;
using System.Text;
using System.Threading.Tasks;
using System;

namespace TeamCloud.Adapters.Kubernetes;

public static class KubernetesExtensions
{
    private static readonly Regex KubernetesNamespaceExpression = new Regex("[a-z0-9][-a-z0-9]*[a-z0-9]");

    public static IRuleBuilderOptions<T, string> MustBeKubernetesNamespace<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
        => ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeKubernetesNamespace)
                .WithMessage("'{PropertyName}' must be a valid Kubernetes namespace.");

    private static bool BeKubernetesNamespace(string kubernetesNamespace)
    {
        if (!string.IsNullOrEmpty(kubernetesNamespace))
        {
            var matches = KubernetesNamespaceExpression.Matches(kubernetesNamespace);

            return kubernetesNamespace.Equals(string.Join('.', matches.Select(m => m.Value)));
        }

        return false;
    }

    public static async Task<K8SConfiguration> CreateClusterConfigAsync(this IKubernetes kubernetes, V1ServiceAccount serviceAccount)
    {
        if (kubernetes is null)
            throw new ArgumentNullException(nameof(kubernetes));

        if (serviceAccount is null)
            throw new ArgumentNullException(nameof(serviceAccount));

        var serviceAccountSecret = await serviceAccount.Secrets
            .ToAsyncEnumerable()
            .SelectAwait(s => new ValueTask<V1Secret>(kubernetes.ReadNamespacedSecretAsync(s.Name, serviceAccount.Namespace())))
            .FirstAsync(s => s.Type.Equals("kubernetes.io/service-account-token"))
            .ConfigureAwait(false);

        var clusterName = kubernetes.BaseUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
        var clusterUser = serviceAccount.Name();
        var clusterContext = Guid.NewGuid().ToString();

        return new K8SConfiguration()
        {
            ApiVersion = "v1",
            Kind = "Config",
            Clusters = new Cluster[]
            {
                    new Cluster()
                    {
                        Name = clusterName,
                        ClusterEndpoint = new ClusterEndpoint()
                        {
                            CertificateAuthorityData = Convert.ToBase64String(serviceAccountSecret.Data["ca.crt"]),
                            Server = kubernetes.BaseUri.ToString().TrimEnd('/')
                        }
                    }
            },
            Users = new User[]
            {
                    new User()
                    {
                        Name = clusterUser,
                        UserCredentials = new UserCredentials()
                        {
                            ClientKeyData = Convert.ToBase64String(serviceAccountSecret.Data["ca.crt"]),
                            Token = Encoding.UTF8.GetString(serviceAccountSecret.Data["token"])
                        }
                    }
            },
            Contexts = new Context[]
            {
                    new Context()
                    {
                        Name = clusterContext,
                        ContextDetails = new ContextDetails()
                        {
                            Cluster = clusterName,
                            Namespace = serviceAccount.Name(),
                            User = clusterUser
                        }
                    }
            },
            CurrentContext = clusterContext
        };

    }

    public static async Task<KubernetesClientConfiguration> CreateClientConfigAsync(this IKubernetes kubernetes, V1ServiceAccount serviceAccount)
    {
        if (kubernetes is null)
            throw new ArgumentNullException(nameof(kubernetes));

        if (serviceAccount is null)
            throw new ArgumentNullException(nameof(serviceAccount));

        var clusterConfig = await kubernetes
            .CreateClusterConfigAsync(serviceAccount)
            .ConfigureAwait(false);

        return KubernetesClientConfiguration.BuildConfigFromConfigObject(clusterConfig);
    }
}
