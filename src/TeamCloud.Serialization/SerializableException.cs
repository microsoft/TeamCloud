/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace TeamCloud.Serialization
{

    [Serializable]
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Explicit non-standard implementation.")]
    public class SerializableException : Exception
    {
        private readonly string ClassNameOriginal;

        internal SerializableException(Exception innerException) : base(innerException?.Message ?? string.Empty, innerException)
        {
            if (InnerException is null)
                throw new ArgumentNullException(nameof(innerException));

            ClassNameOriginal = innerException.GetType().FullName;
        }

        protected SerializableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ClassNameOriginal = info.GetString(nameof(ClassNameOriginal));
        }

        private string MessageSuffix => InnerException is null ? null : $" ({InnerException.GetType().FullName})";

        public override IDictionary Data => InnerException?.Data ?? base.Data;

        public override string HelpLink { get => InnerException?.HelpLink ?? base.HelpLink; }

        public override string Message => $"{InnerException?.Message ?? base.Message}{MessageSuffix}";

        public override string Source { get => InnerException?.Source ?? base.Source; }

        public override string StackTrace => InnerException?.StackTrace ?? base.StackTrace;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            if (InnerException is null)
            {
                base.GetObjectData(info, context);
            }
            else
            {
                var innerExceptionInfo = new SerializationInfo(info.ObjectType, new FormatterConverter());

                InnerException.GetObjectData(innerExceptionInfo, context);

                foreach (var innerExceptionInfoEntry in innerExceptionInfo)
                {
                    if (innerExceptionInfoEntry.Name.Equals("ClassName", StringComparison.Ordinal) && innerExceptionInfoEntry.ObjectType == typeof(string))
                        info.AddValue(innerExceptionInfoEntry.Name, this.GetType().FullName, innerExceptionInfoEntry.ObjectType);
                    else if (innerExceptionInfoEntry.Name.Equals("Message", StringComparison.Ordinal) && innerExceptionInfoEntry.ObjectType == typeof(string))
                        info.AddValue(innerExceptionInfoEntry.Name, Message, innerExceptionInfoEntry.ObjectType);
                    else if (innerExceptionInfoEntry.Value is Exception exception && !exception.IsSerializable())
                        info.AddValue(innerExceptionInfoEntry.Name, null, innerExceptionInfoEntry.ObjectType);
                    else
                        info.AddValue(innerExceptionInfoEntry.Name, innerExceptionInfoEntry.Value, innerExceptionInfoEntry.ObjectType);
                }
            }

            info.AddValue(nameof(ClassNameOriginal), ClassNameOriginal);
        }
    }
}
