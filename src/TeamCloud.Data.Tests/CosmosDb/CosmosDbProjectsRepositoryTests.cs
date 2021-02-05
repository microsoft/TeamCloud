/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using TeamCloud.Data.Conditional;
// using TeamCloud.Data.CosmosDb.Core;
// using TeamCloud.Model.Data;
// using Xunit;

// namespace TeamCloud.Data.CosmosDb
// {
//     [Collection(nameof(CosmosDbRepositoryCollection))]
//     public class CosmosDbprojectRepositoryTests : CosmosDbRepositoryTests<CosmosDbProjectRepository>
//     {
//         private readonly CosmosDbRepositoryFixture fixture;

//         public CosmosDbprojectRepositoryTests(CosmosDbRepositoryFixture fixture)
//             : base(new CosmosDbProjectRepository(CosmosDbTestOptions.Instance, new CosmosDbUserRepository(CosmosDbTestOptions.Instance), new CosmosDbProjectLinkRepository(CosmosDbTestOptions.Instance)))
//         {
//             this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
//         }

//         private IEnumerable<User> GetUsers()
//         {
//             yield return new User()
//             {
//                 Id = Guid.NewGuid().ToString()
//             };
//         }

//         [ConditionalFact(ConditionalFactPlatforms.Windows)]
//         public async Task AddProject()
//         {
//             var project = await Repository.AddAsync(new Project()
//             {
//                 Name = Guid.NewGuid().ToString(),
//                 Type = new ProjectTypeDocument()
//                 {
//                     Id = Guid.NewGuid().ToString()
//                 },
//                 Users = GetUsers().ToList()

//             }).ConfigureAwait(false);

//             AssertContainerDocumentMetadata(project);
//         }

//         [ConditionalFact(ConditionalFactPlatforms.Windows)]
//         public async Task UpdateProject()
//         {
//             var project = await Repository.AddAsync(new Project()
//             {
//                 Name = Guid.NewGuid().ToString(),
//                 Type = new ProjectTypeDocument()
//                 {
//                     Id = Guid.NewGuid().ToString()
//                 },
//                 Users = GetUsers().ToList()

//             }).ConfigureAwait(false);

//             AssertContainerDocumentMetadata(project);

//             foreach (var user in GetUsers())
//                 project.Users.Add(user);

//             var project2 = await Repository
//                 .SetAsync(project)
//                 .ConfigureAwait(false);

//             Assert.Equal(project.Id, project2.Id);
//             AssertContainerDocumentMetadata(project2);
//         }

//         [ConditionalFact(ConditionalFactPlatforms.Windows)]
//         public async Task RemoveProject()
//         {
//             var project = await Repository.AddAsync(new Project()
//             {
//                 Name = Guid.NewGuid().ToString(),
//                 Type = new ProjectTypeDocument()
//                 {
//                     Id = Guid.NewGuid().ToString()
//                 },
//                 Users = GetUsers().ToList()

//             }).ConfigureAwait(false);

//             AssertContainerDocumentMetadata(project);

//             await Repository
//                 .RemoveAsync(project)
//                 .ConfigureAwait(false);
//         }


//     }
// }
