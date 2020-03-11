/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace TeamCloud.Orchestration
{

    [Serializable]
    public class FunctionException : Exception
    {
        private readonly Exception innerException;

        internal FunctionException(Exception innerException) : base(innerException.Message)
        {
            this.innerException = innerException ?? throw new ArgumentNullException(nameof(innerException));

            OriginalExceptionType = innerException.GetType();
        }

        protected FunctionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            OriginalExceptionType = info.GetValue(nameof(OriginalExceptionType), typeof(Type)) as Type;
        }

        public Type OriginalExceptionType { get; private set; }

        public override IDictionary Data => InnerException?.Data ?? base.Data;

        public override string Message => InnerException?.Message ?? base.Message;

        public override string StackTrace => InnerException?.StackTrace ?? base.StackTrace;

        public override string Source
        {
            get => base.Source ?? InnerException?.Source;
            set => base.Source = value;
        }

        public override string HelpLink
        {
            get => base.HelpLink ?? InnerException?.HelpLink;
            set => base.HelpLink = value;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (innerException is null)
            {
                base.GetObjectData(info, context);
            }
            else
            {
                innerException.GetObjectData(info, context);
            }

            info.AddValue(nameof(OriginalExceptionType), OriginalExceptionType, typeof(Type));
        }
    }
}
