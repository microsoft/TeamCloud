using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProjectLinkUpdateCommand : OrchestratorCommand<ProjectLinkDocument, OrchestratorProjectLinkUpdateCommandResult>
    {
        public OrchestratorProjectLinkUpdateCommand(UserDocument user, ProjectLinkDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}
