/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data
{
    public interface IDocumentExpander
    {
        public bool CanExpand(IContainerDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (typeof(IDocumentExpander<>).MakeGenericType(document.GetType()).IsAssignableFrom(GetType()))
            {
                var canMethod = typeof(IDocumentExpander<>)
                    .MakeGenericType(document.GetType())
                    .GetMethod(nameof(CanExpand), new Type[] { document.GetType() });

                return (bool)canMethod.Invoke(this, new object[] { document });

            }

            return false;
        }

        public async Task<IContainerDocument> ExpandAsync(IContainerDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (CanExpand(document))
            {
                try
                {
                    var expandMethod = typeof(IDocumentExpander<>)
                        .MakeGenericType(document.GetType())
                        .GetMethod(nameof(ExpandAsync), new Type[] { document.GetType() });

                    var expandTask = (Task)expandMethod.Invoke(this, new object[] { document });

                    await expandTask.ConfigureAwait(false);

                    return (IContainerDocument)expandTask.GetType()
                        .GetProperty(nameof(Task<object>.Result))
                        .GetValue(expandTask);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            throw new NotImplementedException($"Missing document expander implementation IDocumentExpander<{document.GetType().Name}> at {GetType()}");
        }
    }

    public interface IDocumentExpander<T> : IDocumentExpander
        where T : class, IContainerDocument
    {
        bool CanExpand(T document);

        public Task<T> ExpandAsync(T document);
    }
}
