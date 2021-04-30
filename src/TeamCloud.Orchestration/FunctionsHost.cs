using System;

namespace TeamCloud.Orchestration
{
    public interface IFunctionsHost
    {
        string HostUrl { get; }
    }

    public sealed class FunctionsHost : IFunctionsHost
    {
        public static IFunctionsHost Default { get; } = new FunctionsHost();

        private readonly string hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

        private FunctionsHost()
        { }

        public string HostUrl => $"http{(hostName.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) ? "" : "s")}://{hostName}";
    }
}
