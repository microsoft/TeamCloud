using System.Collections.Generic;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data
{
    public interface IDocumentSubscriptionProvider
    {
        IEnumerable<IDocumentSubscription> GetSubscriptions(IContainerDocument document);
    }
}
