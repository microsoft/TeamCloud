/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Reflection;

namespace TeamCloud.Serialization
{
    public static class Extensions
    {
        internal static bool IsSerializable(this Exception exception)
        {
            if (typeof(SerializableException).IsAssignableFrom(exception.GetType())) return true;

            var jsonSerializable = !(exception.GetType().GetCustomAttribute<SerializableAttribute>() is null);

            return jsonSerializable && (exception.InnerException?.IsSerializable() ?? true);
        }

        public static bool IsSerializable(this Exception exception, out Exception serializableException)
        {
            var isSerializable = exception.IsSerializable();

            serializableException = (isSerializable ? exception : new SerializableException(exception));

            return isSerializable;
        }
    }
}
