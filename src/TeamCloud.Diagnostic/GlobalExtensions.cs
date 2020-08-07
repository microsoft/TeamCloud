using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using TeamCloud.Diagnostic.Logging;

namespace TeamCloud.Diagnostic
{
    public static class GlobalExtensions
    {
        public static IHostBuilder UseTeamCloudDiagnostic(this IHostBuilder builder)
            => builder.UseSerilog(CreateLogger(functionHostMode: false), dispose: true);

        public static IApplicationBuilder UseTeamCloudDiagnostic(this IApplicationBuilder builder)
            => builder.UseSerilogRequestLogging(options =>
            {
                // Customize the message template
                options.MessageTemplate = "Handled {RequestPath}";

                // Emit debug-level events instead of the defaults
                options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress.MapToIPv4());
                };
            });

        public static IServiceCollection AddTeamCloudDiagnostic(this IServiceCollection services, bool functionHostMode)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            var configuration = services
                .BuildServiceProvider()
                .GetRequiredService<IConfiguration>();

            return services
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(CreateLogger(functionHostMode, configuration), dispose: true));
        }

        private static Logger CreateLogger(bool functionHostMode, IConfiguration configuration = null)
        {
            var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            var telemetryConfiguration = string.IsNullOrWhiteSpace(instrumentationKey)
                ? new TelemetryConfiguration()
                : new TelemetryConfiguration(instrumentationKey);

            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);

            if (!functionHostMode)
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Console(outputTemplate: "[{Timestamp:M/d/yyyy HH:mm:ss}] {Level:u3} {Message:lj}{NewLine}{Exception}");

                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
                {
                    loggerConfiguration = loggerConfiguration
                        .WriteTo.File(
                        @"D:\home\LogFiles\Application\log.txt",
                        fileSizeLimitBytes: 1000000,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1));
                }
            }

            if (CloudStorageAccount.TryParse(configuration?.GetValue<string>("AzureWebJobsStorage"), out var cloudStorageAccount))
            {
                loggerConfiguration = loggerConfiguration
                      .WriteTo.AzureTableStorageWithProperties(cloudStorageAccount,
                      storageTableName: "Log",
                      keyGenerator: new KeyGenerator(),
                      propertyColumns: new string[] { "MS_FunctionInvocationId", "MS_FunctionName", "MS_Event" });
            }

            return loggerConfiguration.CreateLogger();
        }
    }
}
