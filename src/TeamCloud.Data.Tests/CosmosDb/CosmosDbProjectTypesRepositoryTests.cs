/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Data.Conditional;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProjectTypesRepositoryTests : CosmosDbRepositoryTests<CosmosDbProjectTypesRepository>
    {

        public CosmosDbProjectTypesRepositoryTests()
            : base(new CosmosDbProjectTypesRepository(CosmosDbTestOptions.Default, new CosmosDbProjectsRepository(CosmosDbTestOptions.Default, new CosmosDbUsersRepository(CosmosDbTestOptions.Default))))
        { }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task AddProjectType()
        {
            var projectType = await Repository.AddAsync(new ProjectType()
            {
                Id = Guid.NewGuid().ToString()

            }).ConfigureAwait(false);

            AssertContainerDocumentMetadata(projectType);
        }
    }
}
