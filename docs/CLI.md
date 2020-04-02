# TeamCloud CLI

***\*This file is incomplete. It is a work in progress and will change.\****

The TeamCloud CLI is an [extension](https://docs.microsoft.com/en-us/cli/azure/azure-cli-extensions-overview?view=azure-cli-latest) for the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest).  It can be used by application development teams to create and manage Projects, and by TeamCloud admins to create new TeamCloud instances or manage existing instances.

## Install

To install the Azure CLI TeamCloud extension, simply run the following command:

```sh
az extension add --source https://github.com/microsoft/TeamCloud/releases/download/v0.1.274/tc-0.2.2-py2.py3-none-any.whl -y
```

### Update

To update Azure CLI TeamCloud extension, you must first remove the currently installed version of the extension:

```sh
az extension remove -n tc
```

Then run the install command with the location of the updated extension version:

```sh
az extension add --source https://github.com/microsoft/TeamCloud/releases/download/v0.1.274/tc-0.2.2-py2.py3-none-any.whl -y
```

## Local Development

To use the Azure CLI TeamCloud extension with a locally running TeamCloud instance, first set the default base url to your localhost TeamCloud API endpoint:

```sh
az configure -d tc-base-url=https://localhost:5001
```

Next, you must temporarily disable Azure CLI's connection verification:

macOS or Linux

```sh
export AZURE_CLI_DISABLE_CONNECTION_VERIFICATION=1
```

Windows

```cmd
set AZURE_CLI_DISABLE_CONNECTION_VERIFICATION=1
```
