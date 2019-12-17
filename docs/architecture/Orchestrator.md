# Orchestrator

***\*This file is incomplete. It is a work in progress and will change.\****

The Provider orchestration service (or "the Orchestrator") is responsible for invoking Providers registered on a TeamCloud instance via REST calls.  It also manages the state of each Project and project-specific states for the Providers used by the project.

## Orchestration Functions

Several [Durable Orchestration Functions](durable-functions-overview) handle requests from the API and Events and invoke the Providers services accordingly.

- Register
- Create
- Init
- Delete
