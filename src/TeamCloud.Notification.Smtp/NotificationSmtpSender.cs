/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentEmail.Core;
using HtmlAgilityPack;
using TeamCloud.Azure.Directory;

namespace TeamCloud.Notification.Smtp
{

    public class NotificationSmtpSender : INotificationSmtpSender
    {
        private readonly IFluentEmailFactory emailFactory;
        private readonly IAzureDirectoryService azureDirectoryService;

        public NotificationSmtpSender(IFluentEmailFactory emailFactory, IAzureDirectoryService azureDirectoryService)
        {
            this.emailFactory = emailFactory ?? throw new ArgumentNullException(nameof(emailFactory));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        }

        public Task SendMessageAsync(INotificationMessage notificationMessage)
        {
            if (notificationMessage is null)
                throw new ArgumentNullException(nameof(notificationMessage));

            return Task
                .WhenAll(notificationMessage.Recipients.Select(recipient => SendEmailNotificationAsync(recipient, notificationMessage)));

            async Task SendEmailNotificationAsync(INotificationRecipient recipient, INotificationMessage notificationMessage)
            {
                var exceptions = new List<Exception>();
                var addresses = ResolveRecipientEmailAddressesAsync(recipient).ConfigureAwait(false);

                await foreach (var address in addresses)
                {
                    try
                    {
                        var response = await emailFactory.Create()
                            .To(address)
                            .Subject(GetNotificationSubject(notificationMessage))
                            .Body(notificationMessage.Body, notificationMessage.Html)
                            .SendAsync()
                            .ConfigureAwait(false);

                        if (!response.Successful)
                            exceptions.Add(new Exception($"Failed to send mail to user {address}: {string.Join(" / ", response.ErrorMessages)}"));
                    }
                    catch (Exception exc)
                    {
                        exceptions.Add(exc);
                    }
                }

                if (exceptions.Skip(1).Any())
                {
                    throw new AggregateException(exceptions);
                }
                else if (exceptions.Any())
                {
                    throw exceptions.Single();
                }
            }

            string GetNotificationSubject(INotificationMessage notificationMessage)
            {
                var subject = default(string);

                if (notificationMessage.Html && string.IsNullOrWhiteSpace(notificationMessage.Subject))
                {
                    try
                    {
                        var document = new HtmlDocument();

                        document.LoadHtml(notificationMessage.Body);

                        subject = document.DocumentNode.SelectSingleNode("//title")?.InnerText;
                    }
                    catch
                    {
                        // swallow exceptions
                    }
                }

                return string.IsNullOrWhiteSpace(subject) ? notificationMessage.Subject : subject;
            }
        }

        private async IAsyncEnumerable<string> ResolveRecipientEmailAddressesAsync(INotificationRecipient recipient)
        {
            if (recipient.Address?.IsEMail() ?? false)
            {
                yield return recipient.Address;
            }
            else if (recipient.Address?.IsGuid() ?? false)
            {
                var address = await azureDirectoryService
                    .GetMailAddressAsync(recipient.Address)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(address))
                {
                    var memberIds = azureDirectoryService
                        .GetGroupMembersAsync(recipient.Address, true)
                        .ConfigureAwait(false);

                    await foreach (var memberId in memberIds)
                    {
                        address = await azureDirectoryService
                            .GetMailAddressAsync(recipient.Address)
                            .ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(address))
                            yield return address;
                    }

                }
                else
                {
                    yield return address;
                }
            }
        }
    }
}
