/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data;

public abstract class DocumentSubscription : IDocumentSubscription
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>> HandleMethodCache = new ConcurrentDictionary<Type,ConcurrentDictionary<Type, MethodInfo>>();

    private MethodInfo GetHandleMethod(IContainerDocument containerDocument) => HandleMethodCache
        .GetOrAdd(GetType(), _ => new ConcurrentDictionary<Type, MethodInfo>())
        .GetOrAdd(containerDocument.GetType(), containerDocumentType =>
        {
            var subscriberInterface = typeof(IDocumentSubscription<>)
                .MakeGenericType(containerDocument.GetType());

            if (subscriberInterface.IsAssignableFrom(GetType()))
                return subscriberInterface.GetMethod(nameof(HandleAsync), new Type[] { containerDocument.GetType(), typeof(DocumentSubscriptionEvent) });

            return null;
        });

    public virtual bool CanHandle(IContainerDocument containerDocument)
    {
        if (containerDocument is null)
            throw new ArgumentNullException(nameof(containerDocument));

        return GetHandleMethod(containerDocument) is not null;
    }

    public virtual Task HandleAsync(IContainerDocument containerDocument, DocumentSubscriptionEvent subscriptionEvent)
    {
        if (containerDocument is null)
            throw new ArgumentNullException(nameof(containerDocument));

        if (CanHandle(containerDocument))
            return (Task)GetHandleMethod(containerDocument).Invoke(this, new object[] { containerDocument, subscriptionEvent });

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
