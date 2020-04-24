using System;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Audit
{
    public abstract class CommandAuditEntity : TableEntityBase
    {
        public virtual string CommandId { get; set; }
        public string Command { get; set; }

        public virtual string ProjectId { get; set; }
        public string Project { get; set; }

        public CommandRuntimeStatus Status { get; set; } = CommandRuntimeStatus.Unknown;

        public DateTime? Created { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Processed { get; set; }
        public DateTime? Timeout { get; set; }
    }
}
