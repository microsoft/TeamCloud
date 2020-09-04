using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestration.Auditing
{
    public interface ICommandAuditWriter
    {
        Task AuditAsync(ICommand command, ICommandResult commandResult = default, string providerId = default);
    }
}
