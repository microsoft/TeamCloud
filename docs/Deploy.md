# Deploying TeamCloud

To deploy and interact with a TeamCloud instance, you use the TeamCloud CLI. Instructions for installing it can be found [here](CLI.md#install).

## Create an App registration

In order for the TeamCloud Web application to authenticate users and securely call the API, you must create a new app registration in Azure Active Directory. **Follow [these instructions](Web.md#app-registration) to create your registration.** Make sure you copy the Client ID, as you'll need it in the next step.

## Deploy a TeamCloud instance

To deploy a TeamCloud instance to Azure, run the following command replacing `<NAME>` with a name you choose to call your instance. It must be globally unique, and will be used as the subdomain for the service endpoint.

For example, if you choose to pass in `myteamcloud` as the `<NAME>`, your TeamCloud website will be at `https://myteamcloud.azurewebsites.net`.

**Note: This command will take 15+ minutes to complete.**

```sh
az tc deploy -n <NAME>
```

> Use `az tc deploy -h` to see all available arguments and examples.

### Set the default TeamCloud URL

When the deploy command completes, the output will contain a message similar to the following:

```log
TeamCloud instance successfully created at: https://myteamcloud.azurewebsites.net
Use `az configure --defaults tc-base-url=https://myteamcloud-api.azurewebsites.net` to configure this as your default TeamCloud instance
```

**:bangbang: IMPORTANT :bangbang:**

**It is highly recommended you follow the instruction and run the `az configure` command to set your default url. Otherwise you'll have to pass this url as an argument into every command.**

## Done

That's it, you're ready to start using TeamCloud.  Use `az tc -h` to explore the CLI, or open up your website to work from there.
