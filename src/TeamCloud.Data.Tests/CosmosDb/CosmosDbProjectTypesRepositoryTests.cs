using TeamCloud.Data.CosmosDb;
using TeamCloud.Data.CosmosDb.Core;
using Xunit;

namespace TeamCloud.Data.Tests
{
    public class CosmosDbProjectTypesRepositoryTests : CosmosDbRepositoryTests<CosmosDbProjectTypesRepository>
    {

        public CosmosDbProjectTypesRepositoryTests()
            : base(new CosmosDbProjectTypesRepository(CosmosDbTestOptions.Default, new CosmosDbProjectsRepository(CosmosDbTestOptions.Default, new CosmosDbUsersRepository(CosmosDbTestOptions.Default))))
        { }

        [Fact]
        public void Test1()
        {

        }
    }
}
