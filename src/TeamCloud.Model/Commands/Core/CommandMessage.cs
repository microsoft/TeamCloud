/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Commands.Core;

public abstract class CommandMessage : ICommandMessage
{
    protected CommandMessage()
    { }

    protected CommandMessage(ICommand command)
    {
        Command = command ?? throw new ArgumentNullException(nameof(command));
    }

    public ICommand Command { get; set; }

    public Guid? CommandId => Command?.CommandId;

    public Type CommandType => Command?.GetType();
}
