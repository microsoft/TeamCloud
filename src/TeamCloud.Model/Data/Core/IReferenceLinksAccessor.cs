/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;

namespace TeamCloud.Model.Data.Core;

public interface IReferenceLinksAccessor<TContext, TContainer>
    where TContext : class, IReferenceLinksAccessor<TContext, TContainer>
    where TContainer : ReferenceLinksContainer<TContext, TContainer>, new()
{
    [JsonProperty("_links", Order = int.MaxValue)]
    TContainer Links { get; set; }
}
