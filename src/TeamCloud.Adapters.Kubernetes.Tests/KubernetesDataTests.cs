using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Serialization;
using TeamCloud.Serialization.Forms;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;
using Xunit;

namespace TeamCloud.Adapters.Kubernetes.Tests
{
    public class KubernetesDataTests
    {
        private readonly IValidatorProvider validatorProvider;

        public KubernetesDataTests()
        {

            var serviceCollection = new ServiceCollection()
                .AddTeamCloudValidationProvider(config =>
                {
                    config.Register(Assembly.GetExecutingAssembly());
                })
                .AddTeamCloudAdapterProvider(config =>
                {
                    config.Register<KubernetesAdapter>();
                });

            validatorProvider = serviceCollection
                .BuildServiceProvider()
                .GetService<IValidatorProvider>();
        }

        private static string GetManifestResourceJson(string specifier = null)
        {
            var resourceName = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .FirstOrDefault(name => name.Equals($"{typeof(KubernetesDataTests).FullName}.{specifier}.json".Replace("..", ".")));

            if (!string.IsNullOrEmpty(resourceName))
            {
                using var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));

                return reader.ReadToEnd();
            }

            return null;
        }
        public static IEnumerable<object[]> GetValidKubernetesDataJson()
        {
            yield return new object[] { GetManifestResourceJson("yaml-valid") };
            yield return new object[] { GetManifestResourceJson("file-valid") };
        }

        [Theory]
        [MemberData(nameof(GetValidKubernetesDataJson))]
        public void Deserialize_Success(string json)
        {
            var data = TeamCloudSerialize.DeserializeObject<KubernetesData>(json);

            data.Validate(validatorProvider, throwOnValidationError: true, throwOnNoValidatorFound: true);
        }
    }
}
