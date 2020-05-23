using System;

namespace TeamCloud.Model.Data.Core
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PartitionKeyAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class UniqueKeyAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DatabaseIgnoreAttribute : Attribute
    { }

}
