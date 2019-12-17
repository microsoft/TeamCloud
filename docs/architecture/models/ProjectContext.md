# ProjectContext

***\*This file is incomplete. It is a work in progress and will change.\****

The `ProjectContext` object represents an aggregate of information from the Project, the TeamCloud instance's [configuration file](../TeamCloudYaml.md), and relevant [ProviderVariables](Provider.md#providervariables).  It is passed in almost all requests sent to the Providers by the Orchestrator.

## Properties

| Property                        | Type   | Description |
|:--------------------------------|:-------|:------------|
| ProjectId                       | String | The unique identifier of the Project |
| ProjectTags                     | Dictionary\<String:String\> | |
| AzureSubscriptionId             | String | The ID of the Azure Subscription to which this project's resources are deployed |
| AzureResourceGroupId            | String | The ID of the Azure Resource Group containing the project's resources |
| AzureResourceGroupName          | String | The name of the Azure Resource Group containing the project's resources |
| AzureRegion                     | String | The default Azure region for the Azure Resource Group and new Resources |
| TeamCloudId                     | String | |
| TeamCloudTags                   | Dictionary\<String:String\> | |
| TeamCloudApplicationInsightsKey | String | |
| ProviderVariables               | List\<[ProviderVariables](ProviderVariables.md)\> | |
