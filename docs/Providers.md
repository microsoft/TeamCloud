# Providers

***\*This file is incomplete. It is a work in progress and will change.\****

In the context of TeamCloud, a Provider represents an abstract implementation of a service that manages a resource or resources (i.e. a GitHub repository or an Azure resource) for a cloud development environment (or TeamCloud Project).

An organization creates and deploys its own Providers or deploys Providers from the [TeamCloud Providers](https://github.com/microsoft/TeamCloud-Providers) repo to Azure.  It then registers the Providers with its TeamCloud instance.  When a development team sends a request to TeamCloud to create a new (or update an existing) Project, TeamCloud invokes each registered Provider to create, update, or delete it's corresponding resource(s).

## Requirements

This section provides a very high level overview of the endpoints (or methods) a Provider must implement.  These endpoints are called by the TeamCloud instance's orchestrator in response to user API calls or environment changes and events.

### Register

This is the initial "handshake" called on the Provider by the TeamCloud instance outside of the context of a specific project.  Upon receiving this request, the Provider returns an object containing its metadata.

### Create

### Init

### Delete

### SetUsers ( -> All Users)

### SetTags ( -> All Tags)

### GetState (arbitrary info implemented by developer)

### HandleEvent (specific to Azure EventGrid?)

### ~~Sync (consider renaming)~~
