/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.Serialization;

namespace TeamCloud.Model.Commands.Serialization
{
    [Serializable]
    public class CommandException : Exception
    {
        public CommandException()
        { }

        public CommandException(string message) : base(message)
        { }

        protected CommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

}
