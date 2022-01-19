/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data;

public abstract class DocumentSubscription : IDocumentSubscription
{
    public virtual bool CanHandle(IContainerDocument containerDocument)
    {
        if (containerDocument is null)
            throw new ArgumentNullException(nameof(containerDocument));

        return typeof(IDocumentSubscription<>).MakeGenericType(containerDocument.GetType()).IsAssignableFrom(GetType());
    }

    public virtual Task HandleAsync(IContainerDocument containerDocument, DocumentSubscriptionEvent subscriptionEvent)
    {
        if (containerDocument is null)
            throw new ArgumentNullException(nameof(containerDocument));

        if (CanHandle(containerDocument))
        {
            return (Task)typeof(IDocumentExpander<>)
                .MakeGenericType(containerDocument.GetType())
                .GetMethod(nameof(HandleAsync), new Type[] { containerDocument.GetType(), typeof(DocumentSubscriptionEvent) })
                .Invoke(this, new object[] { containerDocument, subscriptionEvent });
        }

        throw new NotImplementedException($"Missing document subscription implementation IDocumentSubscription<{containerDocument.GetType().Name}> at {GetType()}");
    }
}

public enum DocumentSubscriptionEvent
{
    Create,
    Update,
    Delete
}

public interface IDocumentSubscription
{
    bool CanHandle(IContainerDocument containerDocument);

    Task HandleAsync(IContainerDocument containerDocument, DocumentSubscriptionEvent subscriptionEvent);
}

public interface IDocumentSubscription<T> : IDocumentSubscription
    where T : class, IContainerDocument, new()
{
    Task HandleAsync(T containerDocument, DocumentSubscriptionEvent subscriptionEvent);
}
