using System;

namespace TeamCloud.Serialization.Encryption
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class EncryptedAttribute : Attribute
    {
    }
}
