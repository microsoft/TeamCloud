/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Notification.Smtp
{
    public static class NotificationSmtpExtensions
    {
        public static IServiceCollection AddTeamCloudNotificationSmtpSender(this IServiceCollection services, INotificationSmtpOptions options = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .TryAddSingleton<INotificationSmtpSender, NotificationSmtpSender>();

            options ??= services.BuildServiceProvider().GetRequiredService<INotificationSmtpOptions>();

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            else if (string.IsNullOrWhiteSpace(options?.SenderAddress))
            {
                throw new ArgumentException($"Option 'SenderAddress' must not null or empty");
            }
            else if (string.IsNullOrWhiteSpace(options?.Host))
            {
                throw new ArgumentException($"Option 'Host' must not null or empty");
            }

            services
                .AddFluentEmail(options.SenderAddress, options.SenderName)
                .AddSmtpSender(() => new SmtpClient(options.Host, options.Port)
                {
                    EnableSsl = options.SSL,
                    Credentials = new NetworkCredential(options.Username, options.Password)
                });

            return services;
        }
    }
}
