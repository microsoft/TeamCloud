using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentResetCommand : CreateCommand<Component, ComponentResetCommandResult>
    {
        public ComponentResetCommand(User user, Component payload) : base(user, payload)
        { }
    }
}
