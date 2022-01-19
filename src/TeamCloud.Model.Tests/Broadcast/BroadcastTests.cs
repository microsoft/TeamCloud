/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using TeamCloud.Serialization;
using Xunit;

namespace TeamCloud.Model.Broadcast;

public class BroadcastTests
{
    [Fact]
    public void Serialize()
    {
        var broadcastMessage = new BroadcastMessage()
        {
            Action = "Custom",
            Timestamp = DateTime.UtcNow,
            Items = new List<BroadcastMessage.Item>()
                {
                    new BroadcastMessage.Item()
                    {
                        Organization = Guid.NewGuid().ToString(),
                        Project = Guid.NewGuid().ToString(),
                        Component = Guid.NewGuid().ToString(),
                        ETag = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                }
        };

        var broadcastJson = TeamCloudSerialize.SerializeObject(broadcastMessage);

        Assert.NotEmpty(broadcastJson);
        Assert.DoesNotContain("$type", broadcastJson);
    }
}
