namespace TeamCloud.Configuration.Options
{
    [Options("Endpoint:Orchestrator")]
    public sealed class EndpointOrchestratorOptions
    {
        public string Url { get; set; }

        public string AuthCode { get; set; }
    }
}
