# TeamCloud Client

***\*This file is incomplete. It is a work in progress and will change.\****

TeamCloud provides a user-facing REST [API](API.md) that enables TeamCloud admins to manage the TeamCloud instance, and application development teams to create and manage Projects.

Organizations can integrate TeamCloud into existing workflows and tools by using the [REST API directly](API.md).  Alternatively, our intent is to provide a TeamCloud Client, possibly in the form of an extension to the Azure CLI (command line interface).

## TeamCloud Extension for Azure CLI

For the initial release of TeamCloud, we believe a TeamCloud [extension](https://docs.microsoft.com/en-us/cli/azure/azure-cli-extensions-overview?view=azure-cli-latest) for the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest) would provide the best user experience.

We are currently [researching the requirements and feasibility](https://github.com/Azure/azure-cli/tree/master/doc/extensions) of Azure CLI extensions, and will update this document accordingly as decisions are made.

## Alternative Options

The following is a list of alternative options to consider for the initial or future (additional) clients for TeamCloud:

- Stand-alone TeamCloud CLI (using [knack](https://github.com/Microsoft/knack))
- Visual Studio Code extension
- Website (deployed as part of a TeamCloud instance)
  