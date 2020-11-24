namespace TeamCloud.Model.Common
{
    public interface IResourceReference
    {
        public string ResourceId { get; set; }

        public ResourceState ResourceState { get; set; }
    }
}
