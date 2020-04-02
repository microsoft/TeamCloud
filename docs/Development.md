# Development

***\*This file is incomplete. It is a work in progress and will change.\****

## Develop Locally

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

[functions-core-tools]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local
[functions-runtime-versions]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-versions
[functions-local-development]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local#local-development-environments
[functions-local-settings]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#local-settings-file
[functions-get-storage]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#get-your-storage-connection-strings
[functions-get-settings-portal]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#portal
[functions-get-settings-cli]:https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#azure-cli
