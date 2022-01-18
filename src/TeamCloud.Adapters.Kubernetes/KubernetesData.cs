using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TeamCloud.Validation;

namespace TeamCloud.Adapters.Kubernetes
{
    public sealed class KubernetesData : IValidatable
    {
        private static readonly Regex DataUrlExpression = new Regex(@"^data:.*;base64,(?<data>.+)$");

        public static string ParseYamlFromDataUrl(string url)
        {
            var yaml = default(string);

            if (!string.IsNullOrWhiteSpace(url))
            {
                var match = DataUrlExpression.Match(url);

                if (match.Success)
                {
                    var base64 = match.Groups
                        .Cast<Group>()
                        .Where(g => g.Success)
                        .FirstOrDefault(g => g.Name.Equals("data"))?.Value;

                    if (!string.IsNullOrEmpty(base64))
                    {
                        var buffer = Convert.FromBase64String(base64);

                        yaml = Encoding.UTF8.GetString(buffer);
                    }
                }
            }

            return yaml;
        }

        public string Namespace { get; set; }

        public KubernetesConfigurationSource Source { get; set; }

        private string file;

        public string File
        {
            get => Source == KubernetesConfigurationSource.File ? file : default;
            set => file = value;
        }

        private string yaml;

        public string Yaml
        {
            get => Source == KubernetesConfigurationSource.Yaml ? yaml : ParseYamlFromDataUrl(File);
            set => yaml = value;
        }
    }

    public enum KubernetesConfigurationSource
    {
        File,

        Yaml
    }
}
