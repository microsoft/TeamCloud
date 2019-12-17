# ProviderVariables

***\*This file is incomplete. It is a work in progress and will change.\****

`ProviderVariables` is a dictionary of arbitrary key/value pairs that are [defined](../TeamCloudYaml.md) in the `teamcloud.yaml` configuration file or returned during runtime by a Provider when invoked (i.e. the object returned by calls to Create or Init).  These variables are stored and passed to other Providers that [declare a dependency](../TeamCloudYaml.md) on the Provider that returned the variables.

TODO: Example use case...

`ProviderVariables` are encrypted before they are stored.  However, when dealing with secure variables, you may consider having the Provider store the secure data in a Key Vault instance and returning the location of that key in the ProviderVariables instead.  Then when that key location is passed to a dependent Provider, have that Provider retrieve the key directly from Key Vault.

## Properties

| Property   | Type   | Description |
|:-----------|:-------|:------------|
| ProviderId | String | The unique identifier of the Provider |
| Variables  | Dictionary\<String:String\> | Arbitrary key/value pairs to be stored by the Project and passed to other Providers |
