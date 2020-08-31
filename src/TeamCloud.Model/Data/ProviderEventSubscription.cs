using System;

namespace TeamCloud.Model.Data
{
    public sealed class ProviderEventSubscription : IEquatable<ProviderEventSubscription>
    {
        public static ProviderEventSubscription All
            => new ProviderEventSubscription() { EventType = "*" };

        public static ProviderEventSubscription ResourceActionCancel
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceActionCancel" };

        public static ProviderEventSubscription ResourceActionFailure
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceActionFailure" };

        public static ProviderEventSubscription ResourceActionSuccess
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceActionSuccess" };

        public static ProviderEventSubscription ResourceDeleteCancel
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceDeleteCancel" };

        public static ProviderEventSubscription ResourceDeleteFailure
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceDeleteFailure	" };

        public static ProviderEventSubscription ResourceDeleteSuccess
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceDeleteSuccess" };

        public static ProviderEventSubscription ResourceWriteCancel
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceWriteCancel" };

        public static ProviderEventSubscription ResourceWriteFailure
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceWriteFailure" };

        public static ProviderEventSubscription ResourceWriteSuccess
            => new ProviderEventSubscription() { EventType = "Microsoft.Resources.ResourceWriteSuccess" };

        public string EventType { get; set; }

        public bool Equals(ProviderEventSubscription other)
            => string.Equals(this.EventType, other.EventType, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProviderEventSubscription);

        public override int GetHashCode()
            => HashCode.Combine(this.GetType(), EventType);
    }
}
