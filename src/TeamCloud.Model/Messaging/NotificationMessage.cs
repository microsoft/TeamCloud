/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamCloud.Notification;
using TeamCloud.Serialization.Compress;

namespace TeamCloud.Model.Messaging;

public abstract class NotificationMessage : INotificationMessage
{
    private static bool TryGetBody<TMessage>(string extension, out string body)
    {
        var resourceName = typeof(TMessage).Assembly
            .GetManifestResourceNames()
            .SingleOrDefault(rn => rn.Equals($"{typeof(TMessage).FullName}.{extension}", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(resourceName))
        {
            body = null;
            return false;
        }
        else
        {
            using var stream = typeof(TMessage).Assembly.GetManifestResourceStream(resourceName);
            using var streamReader = new StreamReader(stream);

            body = streamReader.ReadToEnd();
            return true;
        }
    }

    public static TMessage Create<TMessage>(INotificationRecipient recipient, params INotificationRecipient[] recipients)
        where TMessage : NotificationMessage, new()
    {
        var message = Activator.CreateInstance<TMessage>();

        message.Recipients = recipients.Prepend(recipient).ToArray();

        if (TryGetBody<TMessage>("html", out var htmlBody))
        {
            message.Body = htmlBody;
            message.Html = true;
        }
        else if (TryGetBody<TMessage>("text", out var textBody))
        {
            message.Body = textBody;
            message.Html = false;
        }

        return message;
    }

    public IEnumerable<INotificationRecipient> Recipients { get; set; } = Enumerable.Empty<INotificationRecipient>();

    public string Subject { get; set; }

    [Compress]
    public string Body { get; set; }

    public bool Html { get; set; }
}
