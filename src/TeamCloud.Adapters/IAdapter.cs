/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Adapters
{
    public interface IAdapter
    {
        public bool CanHandle(IContainerDocument containerDocument)
        {
            if (containerDocument is null)
                throw new ArgumentNullException(nameof(containerDocument));

            var type = typeof(IAdapter<>)
                .MakeGenericType(containerDocument.GetType());

            if (type?.IsAssignableFrom(this.GetType()) ?? false)
            {
                return (bool)type
                    .GetMethod(nameof(CanHandle), new Type[] { containerDocument.GetType() })
                    .Invoke(this, new[] { containerDocument });
            }

            return false;
        }
    }

    public interface IAdapter<TContext> : IAdapter
        where TContext : class, IContainerDocument, new()
    {
        bool CanHandle(TContext containerDocument);
    }
}
