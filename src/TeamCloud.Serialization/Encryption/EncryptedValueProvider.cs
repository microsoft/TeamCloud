/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Encryption;

public sealed class EncryptedValueProvider : IValueProvider
{
    public static IDataProtectionProvider DefaultDataProtectionProvider { get; set; }

    private static Type GetMemberType(MemberInfo member) => member switch
    {
        // we support properties with string as value type
        PropertyInfo propertyInfo => propertyInfo.PropertyType,

        // we support fields with string as value type
        FieldInfo fieldInfo => fieldInfo.FieldType,

        // we don't support encryption by default
        _ => null
    };

    private static bool IsSupported(MemberInfo member)
        => GetMemberType(member) == typeof(string);

    private readonly MemberInfo member;
    private readonly IValueProvider innerValueProvider;
    private readonly IDataProtectionProvider dataProtectionProvider;

    public EncryptedValueProvider(MemberInfo member, IValueProvider innerValueProvider, IDataProtectionProvider dataProtectionProvider = null)
    {
        this.member = member ?? throw new ArgumentNullException(nameof(member));
        this.innerValueProvider = innerValueProvider ?? throw new ArgumentNullException(nameof(innerValueProvider));
        this.dataProtectionProvider = dataProtectionProvider ?? DefaultDataProtectionProvider;
    }

    public object GetValue(object target)
    {
        var value = innerValueProvider.GetValue(target);

        if (IsSupported(member) && value is not null)
        {
            Debug.WriteLine($"Encrypt {member.Name} @ {target}");

            value = dataProtectionProvider?
                .CreateProtector(this.GetType().FullName)?
                .Protect(value as string) ?? value;
        }

        return value;
    }

    public void SetValue(object target, object value)
    {
        if (IsSupported(member) && value is not null)
        {
            Debug.WriteLine($"Decrypt {member.Name} @ {target}");

            try
            {
                value = dataProtectionProvider?
                    .CreateProtector(this.GetType().FullName)?
                    .Unprotect(value as string) ?? value;
            }
            catch (CryptographicException)
            {
                var memberType = GetMemberType(member);

                value = memberType?.IsValueType ?? false
                    ? Activator.CreateInstance(memberType)
                    : null;
            }
        }

        innerValueProvider.SetValue(target, value);
    }
}
