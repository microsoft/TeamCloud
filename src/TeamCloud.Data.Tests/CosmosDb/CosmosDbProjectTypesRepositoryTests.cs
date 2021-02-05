/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using TeamCloud.Data.Conditional;
// using TeamCloud.Data.CosmosDb.Core;
// using TeamCloud.Model.Data;
// using Xunit;

// namespace TeamCloud.Data.CosmosDb
// {
//     [Collection(nameof(CosmosDbRepositoryCollection))]
//     public class CosmosDbprojectTypeRepositoryTests : CosmosDbRepositoryTests<CosmosDbProjectTypeRepository>
//     {
//         private readonly CosmosDbRepositoryFixture fixture;

//         public CosmosDbprojectTypeRepositoryTests(CosmosDbRepositoryFixture fixture)
//             : base(new CosmosDbProjectTypeRepository(CosmosDbTestOptions.Instance, new CosmosDbProjectRepository(CosmosDbTestOptions.Instance, new CosmosDbUserRepository(CosmosDbTestOptions.Instance), new CosmosDbProjectLinkRepository(CosmosDbTestOptions.Instance))))
//         {
//             this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
//         }

//         private static string SanitizeName(string name)
//         {
//             var sanitizedName = new StringBuilder(name.Length);

//             foreach (var c in name.ToCharArray())
//             {
//                 if (sanitizedName.Length > 0 && char.IsUpper(c))
//                     sanitizedName.Append('.');

//                 sanitizedName.Append(char.ToLowerInvariant(c));
//             }

//             return sanitizedName.ToString();
//         }

//         private IEnumerable<ProviderReference> GetProviderReferences()
//         {
//             yield return new ProviderReference
//             {
//                 Id = SanitizeName(nameof(CosmosDbprojectTypeRepositoryTests))
//             };
//         }

//         [ConditionalFact(ConditionalFactPlatforms.Windows)]
//         public async Task AddProjectType()
//         {
//             var projectType = await Repository.AddAsync(new ProjectTypeDocument
//             {
//                 Id = SanitizeName(nameof(AddProjectType)),
//                 Region = "EastUS",
//                 Providers = GetProviderReferences().ToList()

//             }).ConfigureAwait(false);

//             AssertContainerDocumentMetadata(projectType);
//         }

//         [ConditionalFact(ConditionalFactPlatforms.Windows)]
//         public async Task UpdateProjectType()
//         {
//             var projectTypeId = SanitizeName(nameof(UpdateProjectType));

//             var projectType = await Repository.AddAsync(new ProjectTypeDocument
//             {
//                 Id = projectTypeId,
//                 Region = "EastUS",
//                 Providers = GetProviderReferences().ToList()

//             }).ConfigureAwait(false);

//             Assert.Equal(projectTypeId, projectType.Id);
//             AssertContainerDocumentMetadata(projectType);

//             projectType.Subscriptions.Add(Guid.NewGuid());

//             var projectType2 = await Repository
//                 .SetAsync(projectType)
//                 .ConfigureAwait(false);

//             Assert.Equal(projectTypeId, projectType2.Id);
//             AssertContainerDocumentMetadata(projectType2);

//             Assert.Equal(projectType.Subscriptions.First(), projectType2.Subscriptions.First());
//         }

//         [ConditionalFact(ConditionalFactPlatforms.Windows)]
//         public async Task RemoveProjectType()
//         {
//             var projectType = await Repository.AddAsync(new ProjectTypeDocument
//             {
//                 Id = SanitizeName(nameof(RemoveProjectType)),
//                 Region = "EastUS",
//                 Providers = GetProviderReferences().ToList()

//             }).ConfigureAwait(false);

//             AssertContainerDocumentMetadata(projectType);

//             await Repository
//                 .RemoveAsync(projectType)
//                 .ConfigureAwait(false);
//         }

//         [ConditionalFact(ConditionalFactPlatforms.Windows)]
//         public async Task GetDefaultProjectType()
//         {
//             var projectIds = new List<string>
//             {
//                 SanitizeName(nameof(GetDefaultProjectType)),
//                 SanitizeName(nameof(GetDefaultProjectType)),
//                 SanitizeName(nameof(GetDefaultProjectType)),
//                 SanitizeName(nameof(GetDefaultProjectType)),
//                 SanitizeName(nameof(GetDefaultProjectType))

//             }.Select((name, index) => $"{name}.{index}");

//             foreach (var projectId in projectIds)
//             {
//                 var projectType = await Repository.AddAsync(new ProjectTypeDocument
//                 {
//                     Id = projectId,
//                     IsDefault = true,
//                     Region = "EastUS",
//                     Providers = GetProviderReferences().ToList()

//                 }).ConfigureAwait(false);

//                 AssertContainerDocumentMetadata(projectType);

//                 var defaultProjectType = await Repository
//                     .GetDefaultAsync()
//                     .ConfigureAwait(false);

//                 AssertContainerDocumentMetadata(defaultProjectType);

//                 Assert.Equal(projectType.Id, defaultProjectType.Id);
//             }

//             foreach (var projectId in projectIds)
//             {
//                 var projectType = await Repository
//                     .GetAsync(projectId)
//                     .ConfigureAwait(false);

//                 AssertContainerDocumentMetadata(projectType);

//                 projectType.IsDefault = true;

//                 projectType = await Repository
//                     .SetAsync(projectType)
//                     .ConfigureAwait(false);

//                 AssertContainerDocumentMetadata(projectType);

//                 var defaultProjectType = await Repository
//                     .GetDefaultAsync()
//                     .ConfigureAwait(false);

//                 AssertContainerDocumentMetadata(defaultProjectType);

//                 Assert.Equal(projectType.Id, defaultProjectType.Id);
//             }
//         }
//     }
// }
