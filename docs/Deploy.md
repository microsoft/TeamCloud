# Deploying TeamCloud

To deploy and interact with a TeamCloud instance, you use the [TeamCloud CLI](CLI.md). Instructions for installing it can be found [here](CLI.md#install).

## Deploy a TeamCloud instance

To deploy a TeamCloud instance to Azure, run the following command replacing `<NAME>` with a name you choose to call your instance. It must be globally unique, and will be used as the subdomain for the service endpoint.

For example, if you choose to pass in `myteamcloud` as the `<NAME>`, your TeamCloud instance service endpoint will be `https://myteamcloud.azurewebsites.net`.

This command will take several minutes to complete.

```sh
az tc deploy -n <NAME>
```

Note: If you are logged in to multiple subscriptions, you may need to specify the subscription by passing in additional arguments. To see all available arguments and examples, run `az tc deploy -h`.

### Set the default TeamCloud URL

When the deploy command completes, the output will contain a message similar to the following:

```log
TeamCloud instance successfully created at: https://myteamcloud.azurewebsites.net
Use `az configure --defaults tc-base-url=https://myteamcloud.azurewebsites.net` to configure this as your default TeamCloud instance
```

It is **highly recommended** you follow the instruction and run the `az configure` command to set your default url. Otherwise you'll have to pass this url as an argument into every command.

## Deploy a Web UI (optional)

Although the CLI exposes all functionality provided by TeamCloud, you can also deploy a client website to interact with your instance.

### Create an App registration

In order for the TeamCloud Web application to authenticate users and securely call the API, you must create a new app registration in Azure Active Directory. Follow [these instructions](Web.md#app-registration) to create your registration. Make sure you copy the Client ID, as you'll need it in the next step.

### Deploy the Web App

Just like the instance itself, the web app is deployed using the CLI. Run the following command replacing `<CLIENT-ID>` with the Client ID of your App registration.

```sh
az tc app deploy -c <CLIENT-ID>
```

Use `az tc app deploy -h` to see all available arguments and examples.

## Deploy Providers

Once you have your TeamCloud instance up and running, you'll need to deploy at least one provider. A provider is responsible for managing one or more resources for a project. For example, the Azure DevTestLabs provider is responsible for creating a new DevTestLabs instance for each project.

Again, you use the CLI to deploy providers. Run the following command for each provider you want to deploy, replacing `<NAME>` with the ID of the provider. Available providers include:

- azure.appinsights
- azure.devtestlabs
- azure.devops
- github

```sh
az tc provider deploy -n <NAME>
```

Use `az tc provider deploy -h` to see all available arguments and examples.

## Create a Project Type

The last thing you need to do to get your TeamCloud instance ready to create projects is create at least one project type. A project type is template that defines the "configuration" for new projects. It defines things like which providers will be used by the project, and which Azure subscriptions and location the providers will create their resources.

To create a project type named "simple", run the following command replacing `<SUB-ID>` with your Azure subscription ID (or IDs) and the providers with the providers you deployed.

```sh
az tc project-type create -n simple --subscriptions <SUB-ID> --provider github --provider azure.devtestlabs
```

Use `az tc project-type create -h` to see all available arguments and examples.
