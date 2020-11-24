namespace TeamCloud.Model.Data.Core
{
    public interface IResourceLink
    {
        public string ResourceId { get; set; }

        public ResourceState ResourceState { get; set; }
    }
}
