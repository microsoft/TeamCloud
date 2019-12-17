# Project

***\*This file is incomplete. It is a work in progress and will change.\****

## Properties

| Property                        | Type   | Description |
|:--------------------------------|:-------|:------------|
| Id                              | String | The unique identifier of the Project |
| Name                            | String | |
| Tags                            | Dictionary\<String:String\> | |
| Identity                        | [Identity](Identity.md) | |
| TeamCloudId                     | String | |
| TeamCloudApplicationInsightsKey | String | |
| Providers                       | List\<ProjectProvider\> | |
| AzureSubscriptionId             | String | The ID of the Azure Subscription to which this project's resources are deployed |
| AzureResourceGroupId            | String | The ID of the Azure Resource Group containing the project's resources |
| AzureResourceGroupName          | String | The name of the Azure Resource Group containing the project's resources |
| AzureRegion                     | String | The default Azure region for the Azure Resource Group and new Resources |
| Users                           | List\<[User](User.md)\> | User objects representing users with access to this project |
