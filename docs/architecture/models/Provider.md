# Provider

***\*This file is incomplete. It is a work in progress and will change.\****

## ProviderDefinition

A `ProviderDefinition` object for each Provider in a TeamCloud instance is defined in the [`teamcloud.yaml`](../TeamCloudYaml.md) configuration file and imported to the TeamCloud's App Configuration service.

### ProviderDefinition Properties

| Property           | Type   | Description |
|:-------------------|:-------|:------------|
| Id                 | String | The unique identifier of the Provider (a-z and period) |
| AuthKey            | String | The authorization key required to securely invoke the Provider service |
| Location           | Url    | The URL where the Provider is hosted, used by the orchestrator to call this Provider. |
| Optional           | Bool   | Whether this Provider will be included in ever Project initially or can be added later |
| InitDependencies   | List\<String\> | Provider Ids that this Provider's Init call is dependent on |
| CreateDependencies | List\<String\> | Provider Ids that this Provider's Create call is dependent on |
| EventSubscriptions | List\<String\> | Provider Ids that this Provider's should receive events for |
| Variables          | Dictionary\<String:String\> | Key/Value pairs defined on the Provider defined in the [`teamcloud.yaml`](../TeamCloudYaml.md) |

## ProviderConfiguration (Handshake response)

A `ProviderConfiguraiton` object is returned by the Provider with the TeamCloud's orchestrator invokes the Provider's Register endpoint (sometimes referred to as the "handshake")

### ProviderConfiguration Properties

| Property | Type   | Description |
|:---------|:-------|:------------|
| Identity | [Identity](Identity.md) | TODO... |
