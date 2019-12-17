# TeamCloud API

***\*This file is incomplete. It is a work in progress and will change.\****

TeamCloud provides a user-facing REST API that enables TeamCloud admins to manage the TeamCloud instance, and application development teams to create and manage Projects.

Organizations either use the [TeamCloud Client](Client.md) or integrate TeamCloud into existing workflows and tools by using the REST API directly.

This document will initially contain the high-level design for the TeamCloud API.  Documentation on using the API will be added in the future.

## TeamCloud Admin API

### /config/

- Must be a User on the TeamCloud instance with the Admin role to call the TeamCloud Config API.

##### GET

- Returns: The [teamcloud.yaml](TeamCloudYaml.md) configuration file currently used by the TeamCloud instance
  
##### POST

- Payload: An updated [teamcloud.yaml](TeamCloudYaml.md) configuration file to use by the TeamCloud instance

### /users/

- Must be a User on the TeamCloud instance with the Admin role to call the TeamCloud Users API.

##### GET

- Returns: An array of [User](architecture/models/User.md) objects representing the TeamCloud instance's TeamCloud Users

##### POST

- Payload: An array of [UserDefinition](architecture/models/User.md#userdefinition) objects representing Users to add to the TeamCloud instance

##### PUT

- Payload: An array of [UserDefinition](architecture/models/User.md#userdefinition) objects representing updated or additional information for existing Users on the TeamCloud instance

##### DELETE

- Payload: An array of strings representing the email addresses of Users to be removed from the TeamCloud instance

### /projects/

##### GET

- Must be a User on the TeamCloud instance with the Admin role to call this API
- Returns: An array of [Project](architecture/models/Project.md) objects representing the TeamCloud instance's Projects

##### POST

- Must be a User on the TeamCloud instance with either the Admin or Creator role to call this API
- Payload: A [ProjectDefinition](architecture/models/Project.md#projectdefinition) objects representing Users to add to the TeamCloud instance

##### DELETE

- Must be a User on the TeamCloud instance with the Admin role, or with Creator role and have the Owner role on the Project to call this API
- Payload: A string representing the Project ID of the Project to be removed from the TeamCloud instance

### /providers/

- Adding, updating, and removing Providers is done through the Config API.
- Must be a User on the TeamCloud instance with the Admin role to call the TeamCloud Providers API.

##### GET

- Returns: An array of [Provider](architecture/models/Provider.md) objects representing the TeamCloud instance's registered Providers

## TeamCloud Project API

### /projects/{projectId}/tags/

### /projects/{projectId}/users/

### /projects/{projectId}/providers/

### /projects/{projectId}/providers/{providerId}
