namespace TeamCloud.Configuration.Options
{
    [Options("Endpoint:Api")]
    public sealed class EndpointApiOptions
    {
        public string Url { get; set; }

        public string AuthCode { get; set; }
    }
}
