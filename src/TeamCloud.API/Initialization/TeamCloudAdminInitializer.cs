using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamCloud.API.Services;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Initialization
{
    public class TeamCloudAdminInitializer : IHostInitializer
    {
        private readonly IAzureSessionService sessionService;
        private readonly IUsersRepository usersRepository;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly Orchestrator orchestrator;
        private readonly ILoggerFactory loggerFactory;

        public TeamCloudAdminInitializer(IAzureSessionService sessionService, IUsersRepository usersRepository, IWebHostEnvironment hostingEnvironment, Orchestrator orchestrator, ILoggerFactory loggerFactory)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            this.hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        private async Task<string> ResolveAzureCliUserId()
        {
            var token = await new AzureServiceTokenProvider("RunAs=Developer;DeveloperTool=AzureCLI")
                .GetAccessTokenAsync(sessionService.Environment.GetEndpointUrl(AzureEndpoint.ResourceManagerEndpoint))
                .ConfigureAwait(false);

            return new JwtSecurityTokenHandler()
                .ReadJwtToken(token).Payload
                .GetValueOrDefault("oid", StringComparison.OrdinalIgnoreCase) as string;
        }

        public async Task InitializeAsync()
        {
            var log = loggerFactory.CreateLogger(this.GetType());

            if (!hostingEnvironment.IsDevelopment())
            {
                log.LogInformation($"Hosting environment is '{hostingEnvironment.EnvironmentName}' - skip admin initialization");

                return;
            }

            try
            {
                var exists = await usersRepository
                    .ListAdminsAsync()
                    .AnyAsync()
                    .ConfigureAwait(false);

                if (exists)
                {
                    log.LogInformation($"Environment is initialized with at least 1 admin user.");

                    return;
                }

                var user = new UserDocument
                {
                    Id = await ResolveAzureCliUserId().ConfigureAwait(false),
                    Role = TeamCloudUserRole.Admin,
                    UserType = UserType.User
                };

                var command = new OrchestratorTeamCloudUserCreateCommand(user, user);

                var commandResult = await orchestrator
                    .ExecuteAsync(command)
                    .ConfigureAwait(false);

                if (commandResult.RuntimeStatus == CommandRuntimeStatus.Completed)
                {
                    log.LogInformation($"Initialized environment with user '{user.Id}' as admin");
                }
                else
                {
                    log.LogWarning($"Failed to initialize environment with user '{user.Id}': {commandResult.Errors.FirstOrDefault()?.Message}");
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to process initializer {this.GetType().Name}: {exc.Message}");
            }
        }
    }
}
