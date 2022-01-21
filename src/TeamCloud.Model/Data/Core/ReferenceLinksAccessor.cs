/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Data.Core;

public abstract class ReferenceLinksAccessor<TContext, TContainer>
    : IReferenceLinksAccessor<TContext, TContainer>
    where TContext : class, IReferenceLinksAccessor<TContext, TContainer>
    where TContainer : ReferenceLinksContainer<TContext, TContainer>, new()
{
    private TContainer links;

    public TContainer Links
    {
        get => links ??= Activator.CreateInstance<TContainer>().SetContext(this as TContext);
        set => links = value?.SetContext(this as TContext) ?? throw new ArgumentNullException(nameof(value));
    }
}
