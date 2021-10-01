using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders
{
    public sealed class ProjectExpander : DocumentExpander,
        IDocumentExpander<Project>
    {
        private readonly IUserRepository userRepository;

        public ProjectExpander(IUserRepository userRepository, TelemetryClient telemetryClient) : base(false, telemetryClient)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task ExpandAsync(Project document)
        {
           var users = await userRepository
                .ListAsync(document.Organization, document.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            document.Users = users;
        }
    }
}
