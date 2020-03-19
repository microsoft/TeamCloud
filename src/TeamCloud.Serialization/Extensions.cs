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
        public static bool IsSerializable(this Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            if (typeof(SerializableException).IsAssignableFrom(exception.GetType())) 
                return true;

            var jsonSerializable = !(exception.GetType().GetCustomAttribute<SerializableAttribute>() is null);

            return jsonSerializable && (exception.InnerException?.IsSerializable() ?? true);
        }

        public static bool IsSerializable(this Exception exception, out Exception serializableException)
        {
            var isSerializable = exception.IsSerializable();

            serializableException = (isSerializable ? null : new SerializableException(exception));

            return isSerializable;
        }

        public static Exception AsSerializable(this Exception exception)
            => exception.IsSerializable() ? exception : new SerializableException(exception);
    }
}
