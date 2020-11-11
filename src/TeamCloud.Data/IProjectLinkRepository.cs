// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System.Collections.Generic;
// using System.Threading.Tasks;
// using TeamCloud.Model.Data;

// namespace TeamCloud.Data
// {
//     public interface IProjectLinkRepository
//     {
//         Task<ProjectLink> GetAsync(string projectId, string id);

//         IAsyncEnumerable<ProjectLink> ListAsync(string projectId);

//         Task<ProjectLink> AddAsync(ProjectLink projectLink);

//         Task<ProjectLink> SetAsync(ProjectLink projectLink);

//         Task<ProjectLink> RemoveAsync(ProjectLink projectLink);

//         Task RemoveAsync(string projectId);

//         Task RemoveAsync(string projectId, string id);
//     }
// }
