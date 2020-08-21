using System;
using System.Threading.Tasks;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public interface IOrchestratorCommandHandler
    {
        public bool CanHandle(IOrchestratorCommand orchestratorCommand)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            return typeof(IOrchestratorCommandHandler<>)
                .MakeGenericType(orchestratorCommand.GetType())
                .IsAssignableFrom(GetType());
        }

        public Task<ICommandResult> HandleAsync(IOrchestratorCommand orchestratorCommand)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            if (CanHandle(orchestratorCommand))
            {
                var handleMethod = typeof(IOrchestratorCommandHandler<>)
                    .MakeGenericType(orchestratorCommand.GetType())
                    .GetMethod(nameof(HandleAsync), new Type[] { orchestratorCommand.GetType() });

                return (Task<ICommandResult>)handleMethod
                    .Invoke(this, new object[] { orchestratorCommand });
            }

            throw new NotImplementedException($"Missing orchestrator command handler implementation IOrchestratorCommandHandler<{orchestratorCommand.GetType().Name}> at {GetType()}");
        }
    }

    public interface IOrchestratorCommandHandler<T> : IOrchestratorCommandHandler
        where T : class, IOrchestratorCommand
    {
        Task<ICommandResult> HandleAsync(T orchestratorCommand);
    }
}
