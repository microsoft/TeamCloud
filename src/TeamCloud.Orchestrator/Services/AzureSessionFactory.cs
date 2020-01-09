
using AZFluent = Microsoft.Azure.Management.Fluent;
using RMFluent = Microsoft.Azure.Management.ResourceManager.Fluent;
using System;
using TeamCloud.Configuration.Options;


namespace TeamCloud.Orchestrator.Services
{
    public interface IAzureSessionFactory
    {
        AZFluent.Azure.IAuthenticated CreateSession();

        AZFluent.IAzure CreateSession(Guid subscriptionId);
    }

    public class AzureSessionFactory : IAzureSessionFactory
    {
        private readonly Lazy<AZFluent.Azure.IAuthenticated> session;

        public AzureSessionFactory(AzureRMOptions azureRMOptions)
        {
            if (azureRMOptions is null)
                throw new ArgumentNullException(nameof(azureRMOptions));

            session = new Lazy<AZFluent.Azure.IAuthenticated>(() => {

                var credentials = new RMFluent.Authentication.AzureCredentialsFactory()
                    .FromServicePrincipal(azureRMOptions.ClientId, azureRMOptions.ClientSecret, azureRMOptions.TenantId, RMFluent.AzureEnvironment.AzureGlobalCloud);

                return AZFluent.Azure.Authenticate(credentials);
            });
        }

        public AZFluent.Azure.IAuthenticated CreateSession()
            => session.Value;

        public AZFluent.IAzure CreateSession(Guid subscriptionId)
            => CreateSession().WithSubscription(subscriptionId.ToString());
    }
}
