# Deploying TeamCloud

## Install the TeamCloud CLI

The TeamCloud CLI is an [extension](https://docs.microsoft.com/en-us/cli/azure/azure-cli-extensions-overview?view=azure-cli-latest) for the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest).  Instructions for installing it can be found [here](CLI.md#install).

## Deploy a TeamCloud instance

To deploy a TeamCloud instance to Azure, run the following command replacing `<NAME>` with a name you choose to call your instance.  This must be globally unique, and will be used as the subdomain for the service endpoint.  For example, if you choose to pass in `myteamcloud` as the name, your TeamCloud instance service endpoint will be `https://myteamcloud.azurewebsites.net`.

Note: This command will take several minutes to complete.

```sh
az tc deploy -n <NAME>
```

_If you are logged in to multiple subscriptions, you may need to specify the subscription by passing in additional arguments.  To see all available arguments, run `az tc deploy -h`._

### Set the default TeamCloud URL

When the command completes the output will contain a message similar to the following:

```log
TeamCloud instance successfully created at: https://myteamcloud.azurewebsites.net
Use `az configure --defaults tc-base-url=https://myteamcloud.azurewebsites.net` to configure this as your default TeamCloud instance
```

It is **highly recommended** you follow the instruction and run the `az configure` command to set your default url.  Otherwise you'll have to pass this url as an argument into every command.

## Deploy a Web UI (optional)

Although the TeamCloud CLI exposes all functionality provided by TeamCloud, you can also deploy a client website to interact with your instance.

### Create an App registration

In order for the TeamCloud Web application to authenticate users and securely call the API, you must create a new app registration in Azure Active Directory.  Follow [these instructions](Web.md#app-registration) to create your registration.  Make sure you copy the Client ID, as you'll need it in the next step.

### Deploy the Web App

Just like the instance itself, the web app is deployed using the CLI.  Run the following command replacing `<CLIENT-ID>` with the Client ID of your App registration.

```sh
az tc app deploy -c <CLIENT-ID>
```
