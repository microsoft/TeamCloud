/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.TeamFoundation.SourceControl.WebApi;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Services
{
    internal static class DevOpsExtensions
    {
        public static GitVersionDescriptor VersionDescriptor(this RepositoryReference repository)
            => new GitVersionDescriptor
            {
                Version = repository.Ref,
                VersionType = GitVersionType.Commit
            };
    }
}
