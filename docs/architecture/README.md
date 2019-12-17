# TeamCloud Architecture

***\*This file is incomplete. It is a work in progress and will change.\****

TeamCloud is a tool that enables enterprise IT organizations to provide application development teams "self-serve" access to policy compliant, secure cloud development environments.  This folder contains high-level architecture documentation.

There are several components that make up the TeamCloud solution:

## TeamCloud Instance

At the center of the tool is a TeamCloud instance (the source code in this repository).  An enterprise deploys a single TeamCloud instance, along with one or more Providers, to an Azure subscription managed by its IT organization.

A TeamCloud instance is composed of two parts:

1. A user-facing [REST API](../API.md) that enables TeamCloud admins to manage the TeamCloud instance, and application development teams to create and manage Projects.
2. An internal [orchestration service](Orchestrator.md) (sometimes referred to as "the orchestrator") that communicates with one or more [Providers](../Providers.md) responsible for creating and managing resources for a Project.

Together, the TeamCloud instance and its registered Providers define a template for a policy-compliant, secure, cloud development environment, which software development teams can create on-demand.

### TeamCloud Azure Resources

A TeamCloud instance is made up of the following Azure resources:

- [API Management][api-management]
- [App Configuration][app-configuration]
- [Function Apps][function-apps]
- [Storage Account][storage-account]
- [Key Vault][key-vault]
- [Event Grid][event-grid]
- [Application Insights][application-insights]

![Azure architecture diagram](./TeamCloud.png)

These resources are deployed by an ARM template in this repo.  For deployment instructions, see the [deployment documentation](../Deploy.md).

## Projects

A TeamCloud instance and its registered Providers define a template for a policy-compliant, secure, cloud development environment, which software development teams can create on-demand.  In the context of TeamCloud, these cloud development environments are called Projects.

## Providers

A Provider is responsible for managing one or more resources for a Project.  For example, an organization may implement an "Azure Key Vault Provider" responsible for creating a new Key Vault instance for each Project.  Another example would be a "GitHub repo provider" that creates an associated source code repository for each Project.

Providers are registered with a TeamCloud instance and invoked by the Orchestrator when a Project is created or changed.  Any service that implements [required REST endpoints](../Providers.md) can be [registered as a Provider](../TeamCloudYaml.md).

[api-management]:https://azure.microsoft.com/en-us/services/api-management/
[app-configuration]:https://azure.microsoft.com/en-us/services/app-configuration/
[function-apps]:https://azure.microsoft.com/en-us/services/functions/
[storage-account]:https://azure.microsoft.com/en-us/services/storage/
[key-vault]:https://azure.microsoft.com/en-us/services/key-vault/
[event-grid]:https://azure.microsoft.com/en-us/services/event-grid/
[application-insights]:https://azure.microsoft.com/en-us/services/monitor/
