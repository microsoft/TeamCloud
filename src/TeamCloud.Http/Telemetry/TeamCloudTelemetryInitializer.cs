using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace TeamCloud.Http.Telemetry
{
    public sealed class TeamCloudTelemetryInitializer : ITelemetryInitializer
    {
        private readonly Assembly assembly;

        internal TeamCloudTelemetryInitializer(Assembly assembly)
        {
            this.assembly = assembly ?? throw new System.ArgumentNullException(nameof(assembly));
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is null)
                throw new System.ArgumentNullException(nameof(telemetry));

            telemetry.Context.Cloud.RoleName = assembly.GetName().Name;
        }
    }
}
