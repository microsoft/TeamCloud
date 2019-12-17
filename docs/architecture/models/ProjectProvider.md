# ProjectProvider

***\*This file is incomplete. It is a work in progress and will change.\****

The `ProjectProvider` [Durable Entity](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-dotnet-entities) represents a Provider's state in the context of a specific Project.

## Properties

| Property    | Type   | Description |
|:------------|:-------|:------------|
| [Id](#id)   | String | The unique identifier of the `ProjectProvider`, constructed by combining the associated Project and Provider IDs, separated by a colon (`:`).
| Created     | Bool   | |
| Initialized | Bool   | |

### Id

A combination of the associated Project and Provider IDs, separated by a colon (`:`).

For example if the Project ID is `3127c4c9-bf2d-4453-bb4a-df7fb2390bc5`, and the Provider ID is `providers.azure.keyvault`, the `ProjectProvider`s Id would be:

```text
3127c4c9-bf2d-4453-bb4a-df7fb2390bc5:providers.azure.keyvault
```

The `ProjectProvider` Id corresponds to the Durable Entity's [Entity Key](entity-id), which is used by Orchestration Functions to address the specific Entity.  Constructing the Id in this way allows the calling Orchestration Functions to reliably reconstruct the Id as needed.

[entity-id]:https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-entities#entity-id
