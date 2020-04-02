# TeamCloud API

***\*This file is incomplete. It is a work in progress and will change.\****

A TeamCloud instance exposes a user-facing REST API that enables application development teams to create and manage Projects, and TeamCloud admins to manage the TeamCloud instance itself.

Organizations either use the [TeamCloud CLI](CLI.md) to interface with this REST API, or integrate TeamCloud into existing workflows and tools by using the REST API directly.

Once deployed, TeamCloud instances provide an API explorer that can be found at **_`/swagger`_**.

Below is a summary of the REST API.

## Projects

- **`GET` _`​/api​/projects`_**  Gets all Projects.

- **`POST` _`​/api​/projects`_**  Creates a new Project.

- **`GET` _`​/api​/projects​/{projectNameOrId}`_**  Gets a Project by Name or ID.

- **`DELETE` _`​/api​/projects​/{projectNameOrId}`_**  Deletes a Project.

## Project Users

- **`GET` _`​/api​/projects​/{projectId}​/users`_**  Gets all Users for a Project.

- **`POST` _`​/api​/projects​/{projectId}​/users`_**  Creates a new Project User

- **`PUT` _`​/api​/projects​/{projectId}​/users`_**  Updates an existing Project User.

- **`GET` _`​/api​/projects​/{projectId}​/users​/{userNameOrId}`_**  Gets a Project User by ID or email address.

- **`DELETE` _`​/api​/projects​/{projectId}​/users​/{userNameOrId}`_**  Deletes an existing Project User.

## Project Tags

- **`GET` _`​/api​/projects​/{projectId}​/tags`_**  Gets all Tags for a Project.

- **`POST` _`​/api​/projects​/{projectId}​/tags`_**  Creates a new Project Tag.

- **`PUT` _`​/api​/projects​/{projectId}​/tags`_**  Updates an existing Project Tag.

- **`GET` _`​/api​/projects​/{projectId}​/tags​/{tagKey}`_**  Gets a Project Tag by Key.

- **`DELETE` _`​/api​/projects​/{projectId}​/tags​/{tagKey}`_**  Deletes an existing Project Tag.

## Providers

- **`GET` _`​/api​/providers`_**  Gets all Providers.

- **`POST` _`​/api​/providers`_**  Creates a new Provider.

- **`PUT` _`​/api​/providers`_**  Updates an existing Provider.

- **`GET` _`​/api​/providers​/{providerId}`_**  Gets a Provider by ID.

- **`DELETE` _`​/api​/providers​/{providerId}`_**  Deletes an existing Provider.

## Project Types

- **`GET` _`​/api​/projectTypes`_**  Gets all Project Types.

- **`POST` _`​/api​/projectTypes`_**  Creates a new Project Type.

- **`PUT` _`​/api​/projectTypes`_**  Updates an existing Project Type.

- **`GET` _`​/api​/projectTypes​/{projectTypeId}`_**  Gets a Project Type by ID.

- **`DELETE` _`​/api​/projectTypes​/{projectTypeId}`_**  Deletes a Project Type.

## TeamCloud Users

- **`GET` _`​/api​/users`_**  Gets all TeamCloud Users.

- **`POST` _`​/api​/users`_**  Creates a new TeamCloud User.

- **`PUT` _`​/api​/users`_**  Updates an existing TeamCloud User.

- **`GET` _`​/api​/users​/{userNameOrId}`_**  Gets a TeamCloud User by ID or email address.

- **`DELETE` _`​/api​/users​/{userNameOrId}`_**  Deletes an existing TeamCloud User.

## TeamCloud Tags

- **`GET` _`​/api​/tags`_**  Gets all Tags for a TeamCloud Instance.

- **`POST` _`​/api​/tags`_**  Creates a new TeamCloud Tag.

- **`PUT` _`​/api​/tags`_**  Updates an existing TeamCloud Tag.

- **`GET` _`​/api​/tags​/{tagKey}`_**  Gets a TeamCloud Tag by Key.

- **`DELETE` _`​/api​/tags​/{tagKey}`_**  Deletes an existing TeamCloud Tag.

## Status

- **`GET` _`​/api​/status​/{trackingId}`_**  Gets the status of a long-running operation.

- **`GET` _`​/api​/projects​/{projectId}​/status​/{trackingId}`_**  Gets the status of a long-running operation.

## Admin

- **`POST` _`​/api​/admin​/users`_**  Creates a new TeamCloud User as an Admin.
