﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Serialization;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonConverter(typeof(ContainerDocumentConverter))]
    public interface IContainerDocument : IIdentifiable, IValidatable, ICloneable
    {
        [DatabaseIgnore]
        [JsonProperty("_timestamp")]
        DateTime? Timestamp { get; set; }

        [DatabaseIgnore]
        [JsonProperty("_etag")]
        string ETag { get; set; }
    }

    public abstract class ContainerDocument : IContainerDocument
    {
        [JsonProperty(Required = Required.Always)]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        DateTime? IContainerDocument.Timestamp { get; set; }

        string IContainerDocument.ETag { get; set; }

        public object Clone()
            => Clone(false);

        public virtual object Clone(bool reset)
        {
            var json = TeamCloudSerialize
                .SerializeObject(this);

            var clone = (ContainerDocument) TeamCloudSerialize
                .DeserializeObject(json, GetType());

            if (reset)
                clone.Id = Guid.NewGuid().ToString();

            return clone;
        }

        public override string ToString() => this switch
        {
            IProjectContext projectContext
                => $"/orgs/{projectContext.Organization}/projects/{projectContext.ProjectId}/{this.GetType().Name.ToLowerInvariant()}s/{this.Id}",

            IOrganizationContext organizationContext
                => $"/orgs/{organizationContext.Organization}/{this.GetType().Name.ToLowerInvariant()}s/{this.Id}",

            Organization organization
                => $"/orgs/{organization.Id}",

            IIdentifiable identifiable
                => $"{GetType().Name}[{identifiable.Id}]",

            _ => base.ToString()
        };
    }
}
