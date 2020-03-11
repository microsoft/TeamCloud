/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Runtime.Serialization;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Commands
{

    [Serializable]
    public class ProviderCommandException : CommandException
    {
        public ProviderCommandException()
        { }

        public ProviderCommandException(string message) : base(message)
        { }

        public ProviderCommandException(string message, Exception inner) : base(message, inner)
        { }

        public ProviderCommandException(string message, IProviderCommand providerCommand) : base(message, providerCommand)
        { }

        public ProviderCommandException(string message, IProviderCommand providerCommand, Exception inner) : base(message, providerCommand, inner)
        { }

        protected ProviderCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
