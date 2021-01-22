using System;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Encryption
{
    public sealed class EncryptedValueProvider : IValueProvider
    {
        private readonly MemberInfo member;
        private readonly IValueProvider innerValueProvider;
        private readonly IDataProtectionProvider dataProtectionProvider;

        public EncryptedValueProvider(MemberInfo member, IValueProvider innerValueProvider, IDataProtectionProvider dataProtectionProvider)
        {
            this.member = member ?? throw new ArgumentNullException(nameof(member));
            this.innerValueProvider = innerValueProvider ?? throw new ArgumentNullException(nameof(innerValueProvider));
            this.dataProtectionProvider = dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider));
        }

        public bool Passthrough => member switch
        {
            PropertyInfo propertyInfo => propertyInfo.PropertyType != typeof(string),
            FieldInfo fieldInfo => fieldInfo.FieldType != typeof(string),
            _ => true
        };

        public object GetValue(object target)
        {
            var value = innerValueProvider.GetValue(target);

            if (!Passthrough && value != null)
            {
                value = dataProtectionProvider
                    .CreateProtector(this.GetType().FullName)
                    .Protect(value as string);
            }

            return value;
        }

        public void SetValue(object target, object value)
        {
            if (!Passthrough && value != null)
            {
                value = dataProtectionProvider
                    .CreateProtector(this.GetType().FullName)
                    .Unprotect(value as string);
            }

            innerValueProvider.SetValue(target, value);
        }
    }
}
