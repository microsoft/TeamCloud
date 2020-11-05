/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Git.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Git
{
    public static class GlobalExtensions
    {
        public static bool IsGitHub(this RepositoryReference repo)
            => repo?.Url.Contains("github.com", StringComparison.OrdinalIgnoreCase) ?? throw new ArgumentNullException(nameof(repo));

        public static bool IsDevOps(this RepositoryReference repo)
            => repo is null
             ? throw new ArgumentNullException(nameof(repo))
             : repo.Url.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
            || repo.Url.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase);

        public static bool IsBranch(this Microsoft.TeamFoundation.SourceControl.WebApi.GitRef gitRef)
            => gitRef?.Name?.StartsWith("refs/heads/", StringComparison.Ordinal) ?? throw new ArgumentNullException(nameof(gitRef));

        public static bool IsTag(this Microsoft.TeamFoundation.SourceControl.WebApi.GitRef gitRef)
            => gitRef?.Name?.StartsWith("refs/tags/", StringComparison.Ordinal) ?? throw new ArgumentNullException(nameof(gitRef));

        public static JSchema ToSchema(this IEnumerable<YamlParameter<dynamic>> parameters)
        {
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var schema = new JSchema
            {
                Type = JSchemaType.Object
            };

            foreach (var parameter in parameters)
            {
                var parameterSchema = new JSchema
                {
                    Type = parameter.Type,
                    Default = parameter.Value ?? parameter.Default,
                    ReadOnly = parameter.Readonly,
                    Description = parameter.Name
                };

                parameter.Allowed?.ForEach(a => parameterSchema.Enum.Add(a));

                schema.Properties.Add(parameter.Id, parameterSchema);
            }

            parameters
                .Where(p => p.Required)
                .ToList()
                .ForEach(p => schema.Required.Add(p.Id));

            return schema;
        }

        public static ComponentOffer ToOffer(this ComponentYaml yaml, string repo, string folder)
        {
            if (yaml is null)
                throw new ArgumentNullException(nameof(yaml));

            if (repo is null)
                throw new ArgumentNullException(nameof(repo));

            if (folder is null)
                throw new ArgumentNullException(nameof(folder));

            return new ComponentOffer
            {
                Id = $"{repo}.{folder.Replace(' ', '_').Replace('-', '_')}",
                ProviderId = yaml.Provider,
                DisplayName = folder,
                Description = yaml.Description,
                Scope = yaml.Scope,
                Type = yaml.Type,
                InputJsonSchema = yaml.Parameters.ToSchema().ToString()
            };
        }

        public static ProjectTemplate ToProjectTemplate(this ProjectYaml yaml, RepositoryReference repo, string sha)
        {
            if (yaml is null)
                throw new ArgumentNullException(nameof(yaml));

            if (repo is null)
                throw new ArgumentNullException(nameof(repo));

            return new ProjectTemplate
            {
                Id = repo.Url,
                Repository = new RepositoryReference
                {
                    Url = repo.Url,
                    Token = repo.Token,
                    Version = repo.Version,
                    Ref = sha
                },
                DisplayName = yaml.Name,
                Description = yaml.Description,
                Components = yaml.Components,
                InputJsonSchema = yaml.Parameters.ToSchema().ToString()
            };
        }
    }
}
