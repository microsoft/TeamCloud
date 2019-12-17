# TeamCloud

TeamCloud is a tool that enables enterprise IT organizations to provide application development teams "self-serve" access to secure compliant cloud development environments.

## Concepts

There are several components that make up the TeamCloud solution:

### TeamCloud Instance

At the center of the tool is a TeamCloud instance (the source code in this repository).  An enterprise deploys a single TeamCloud instance, along with one or more Providers, to an Azure subscription managed by its IT organization.

A TeamCloud instance is composed of two parts:

1. A user-facing [REST API](docs/API.md) that enables TeamCloud admins to manage the TeamCloud instance, and application development teams to create and manage Projects.
2. An internal [orchestration service](docs/architecture/Orchestrator.md) (sometimes referred to as "the orchestrator") that communicates with one or more [Providers](docs/Providers.md) responsible for creating and managing resources for a Project.

Together, the TeamCloud instance and its registered Providers define a template for a policy-compliant, secure, cloud development environment, which software development teams can create on-demand.

### Projects

A TeamCloud instance and its registered Providers define a template for a policy-compliant, secure, cloud development environment, which software development teams can create on-demand.  In the context of TeamCloud, these cloud development environments are called Projects.

### Providers

A Provider is responsible for managing one or more resources for a Project.  For example, an organization may implement an "Azure Key Vault Provider" responsible for creating a new Key Vault instance for each Project.  Another example would be a "GitHub repo provider" that creates an associated source code repository for each Project.

Providers are registered with a TeamCloud instance and invoked by the Orchestrator when a Project is created or changed.  Any service that implements [required REST endpoints](docs/Providers.md) can be [registered as a Provider](docs/TeamCloudYaml.md).

# Use

There are a few steps steps required to get a TeamCloud instance configured and deployed:

## Deploy the Azure Resources

A TeamCloud instance is made up of the following Azure resources:

- [App Configuration][app-configuration]
- [Application Insights][application-insights]
- [Event Grid][event-grid]
- [Function Apps][function-apps]
- [Key Vault][key-vault]
- [Storage Account][storage-account]

[![Deploy to Azure][azure-deploy-button]][azure-deploy]

Deploying the Azure resources is as simple as clicking the _"Deploy to Azure"_ link above and filling in a few fields.Below is a brief explanation for filling in each field, please [file an issue](issues/new?labels=docs) if you have questions or require additional help.

- **`Subscription`** Select which Azure subscription you want to use.  It's okay if you only have one choice, or you don't see this option at all.
- **`Resource group`** Unless you have an existing Resource group that you know you want to use, select __Create new__ and provide a name for the new group.  _(a resource group is essentially a parent folder to deploy the resources that make up the TeamCloud instance).
- **`Location`** Select the region to deploy the new resources. You want to choose a region that best describes your location (or your users location).
- **`Function App Name`** Provide a name for your app.  This can be the same name as your Resource group, and will be used as the subdomain for your service endpoint.  For example, if you used `superawesome`, your TeamCloud API would live at `superawesome.azurewebsites.net/api`.
- **Agree & Purchase:** Read and agree to the _TERMS AND CONDITIONS_, then click _Purchase_.

## Deploy the Code to Azure

// TODO...

## Build Locally

***Note: [deploying the Azure resources](#deploy-the-azure-resources) is required even when building and running the source code locally.***

### Development Environment

TeamCloud is built on top of Azure Functions and targets [3.x runtime version][functions-runtime-versions].

[Azure Functions Core Tools][functions-core-tools] **version 3+** is required to build and run the code locally.  Core Tools is already integrated into some development environments.  See the documentation [here][functions-local-development] for guidance on setting up your environment for local development with functions.

### local.settings.json

The [`local.settings.json`][functions-local-settings] file stores app settings, connection strings, and settings used by local development tools. Settings in the local.settings.json file are used only when you're running projects locally.

This file contains keys and connection strings, so it is not committed to this public repo.  After cloning this repo, you'll need to create a new `local.settings.json` file in the `src` folder.  The file's contents should contain the following:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureCosmosDBConnection": "<Your-CosmosDB-Connection-String>",
    "AzureWebJobsStorage": "<Your-WebJobs-Storage-Account-Connection-String>",
    "DurableFunctionsHubStorage": "<Your-TaskHub-Storage-Account-Connection-String>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AppConfigurationConnectionString": "Your-App-Configuration-Connection-String"
  }
}
```

Values for these settings can be [retrieved manually][functions-get-storage].  However, if you used the _"Deploy to Azure"_ button above, each of these settings was automatically added to your Functions App's Settings and can easily be retrieved using the [portal][functions-get-settings-portal] or [CLI][functions-get-settings-cli].

# About

**This project is in active development and will change.**  As the tool becomes ready for use, it will be [versioned](https://semver.org/) and released.

We will do our best to conduct all development openly by [documenting](https://github.com/microsoft/TeamCloud/tree/master/docs) features and requirements, and managing the project using [issues](https://github.com/microsoft/TeamCloud/issues), [milestones](https://github.com/microsoft/TeamCloud/milestones), and [projects](https://github.com/microsoft/TeamCloud/projects).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

[app-configuration]:https://azure.microsoft.com/en-us/services/app-configuration/
[function-apps]:https://azure.microsoft.com/en-us/services/functions/
[storage-account]:https://azure.microsoft.com/en-us/services/storage/
[key-vault]:https://azure.microsoft.com/en-us/services/key-vault/
[event-grid]:https://azure.microsoft.com/en-us/services/event-grid/
[application-insights]:https://azure.microsoft.com/en-us/services/monitor/

[azure-deploy]:https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmicrosoft%2FTeamCloud%2Fmaster%2Fazuredeploy.json
[azure-deploy-button]:https://azuredeploy.net/deploybutton.svg

[functions-core-tools]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local
[functions-runtime-versions]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-versions
[functions-local-development]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local#local-development-environments
[functions-local-settings]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#local-settings-file
[functions-get-storage]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#get-your-storage-connection-strings
[functions-get-settings-portal]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#portal
[functions-get-settings-cli]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#azure-cli
