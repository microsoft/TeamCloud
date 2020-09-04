namespace TeamCloud.Orchestration.Auditing
{
    public interface ICommandAuditWriterOptions
    {
        public string ConnectionString { get; }

        public string StoragePrefix { get; }
    }
}
