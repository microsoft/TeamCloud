using System.Reflection;
using Octokit;

namespace TeamCloud.Adapters.GitHub
{
    internal static class GitHubConstants
    {
        public static readonly string ProductHeaderName = "TeamCloud";

        public static readonly string ProductHeaderVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static ProductHeaderValue ProductHeader => new ProductHeaderValue(ProductHeaderName, ProductHeaderVersion);
    }
}
