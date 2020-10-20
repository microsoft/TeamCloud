using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TeamCloud.Serialization.Converter;
using Xunit;

namespace TeamCloud.Serialization.Tests
{
    public class SerializeNameValueCollectionTests
    {
        [Fact]
        public void DeSerialize_NameValueCollection()
        {
            var nvc = new NameValueCollection();

            for (int i = 1; i <= 10; i++)
                nvc.Add("foo", $"bar{i}");

            var json = JsonConvert.SerializeObject(nvc, new NameValueCollectionConverter());

            for (int i = 1; i <= 10; i++)
                Assert.Contains($"bar{i}", json);

            var nvc2 = JsonConvert.DeserializeObject<NameValueCollection>(json, new NameValueCollectionConverter());

            for (int i = 1; i <= 10; i++)
                Assert.Contains($"bar{i}", nvc2.GetValues("foo"));
        }
    }
}
