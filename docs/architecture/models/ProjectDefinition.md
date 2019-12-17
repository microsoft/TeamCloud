# ProjectDefinition

***\*This file is incomplete. It is a work in progress and will change.\****

The `ProjectDefinition` defines properties of a new Project to be created.  It is used as the payload when calling the API to create a new project.

## Properties

| Property | Type   | Description |
|:---------|:-------|:------------|
| Name     | String | |
| Tags     | Dictionary\<String:String\> | |
| Users    | List\<[UserDefinition](UserDefinition.md)\> | Users to add as Project Users |
