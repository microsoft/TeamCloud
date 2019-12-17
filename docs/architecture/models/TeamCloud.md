# TeamCloud

***\*This file is incomplete. It is a work in progress and will change.\****

The `TeamCloud` object defines a TeamCloud instance.  Much of object is pre-configured and represented in an Azure App Configuration service resource.

## Properties

| Property                        | Type   | Description |
|:--------------------------------|:-------|:------------|
| Id                              | String | The unique identifier of the TeamCloud |
| Name                            | String | |
| Tags                            | Dictionary\<String:String\> | |
| Version                         | String | |
| Identity                        | [Identity](Identity.md) | |
| Projects                        | List\<Project\> | |
| Providers                       | List\<Provider\> | |
| AzureRegion                     | String | The default Azure region for the Azure Resource Group and new Resources |
| AzureResourceGroupId            | String | The ID of the Azure Resource Group containing the project's resources |
| AzureResourceGroupName          | String | The name of the Azure Resource Group containing the project's resources |
| AzureResourceGroupProjectPrefix | String | This value will be prepended to project Resource Group names |
| AzureSubscriptionId             | String | The ID of the Azure Subscription to which this project's resources are deployed |
| AzureSubscriptionPool           | List\<String\> | The IDs of the Azure Subscriptions to which this instance can deploy a new project's resources |
| AzureSubscriptionProjectMax     | Int    | Max number of Projects per Subscription |
| ApplicationInsightsKey          | String | |
| Users                           | List\<[User](User.md)\> | User objects representing users with access to this project |
