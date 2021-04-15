/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Compress
{
    public sealed class CompressValueProvider : IValueProvider
    {
        private static string Decompress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var buffer = Deflate(Convert.FromBase64String(input), CompressionMode.Decompress);
            return Encoding.UTF8.GetString(buffer);
        }

        public static string Compress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var buffer = Deflate(Encoding.UTF8.GetBytes(input), CompressionMode.Compress);
            return Convert.ToBase64String(buffer);
        }

        private static byte[] Deflate(byte[] buffer, CompressionMode compressionMode)
        {
            using var source = compressionMode == CompressionMode.Compress
                ? new MemoryStream()
                : new MemoryStream(buffer);

            using var deflate = new DeflateStream(source, compressionMode);

            if (compressionMode == CompressionMode.Compress)
            {
                deflate.Write(buffer, 0, buffer.Length);
                deflate.Dispose();

                return source.ToArray();
            }
            else
            {
                using var decompressed = new MemoryStream();
                deflate.CopyTo(decompressed);

                return decompressed.ToArray();
            }
        }

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

        public CompressValueProvider(MemberInfo member, IValueProvider innerValueProvider)
        {
            this.member = member ?? throw new ArgumentNullException(nameof(member));
            this.innerValueProvider = innerValueProvider ?? throw new ArgumentNullException(nameof(innerValueProvider));
        }

        public object GetValue(object target)
        {
            var value = innerValueProvider.GetValue(target);

            if (IsSupported(member) && value != null)
            {
                Debug.WriteLine($"Compress {member.Name} @ {target}");

                value = Compress(value as string);
            }

            return value;
        }

        public void SetValue(object target, object value)
        {
            if (IsSupported(member) && value != null)
            {
                Debug.WriteLine($"Decompress {member.Name} @ {target}");

                value = Decompress(value as string);
            }

            innerValueProvider.SetValue(target, value);
        }
    }
}
