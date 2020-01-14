# TeamCloud YAML

This document provides a detailed reference guide to the TeamCloud YAML configuration file, including a catalog of all supported YAML capabilities and available options.

## Structure

- [Version](#version)
- [Azure](#azure)
- [Providers](#providers)
- [Users](#users)
- [Tags](#tags)
- [Variables](#variables)

A [complete example](#complete-example) can be found at the bottom of this document.

### Conventions

Conventions used in this topic:

- To the left of `:` are literal keywords used in pipeline definitions.
- To the right of `:` are data types. These can be primitives like string or references to rich structures defined elsewhere in this topic.
- `[` _datatype_ `]` indicates an array of the mentioned data type. For instance, `[ string ]` is an array of strings.
- `{` _datatype_ `:` _datatype_ `}` indicates a mapping of one data type to another. For instance, `{ string: string }` is a mapping of strings to strings.

### YAML basics

This document covers the schema of an TeamCloud YAML file. To learn the basics of YAML, see [Learn YAML in Y Minutes](https://learnxinyminutes.com/docs/yaml/). Note: TeamCloud doesn't support all features of YAML, such as anchors, complex keys, and sets.

## Version

### Schema

```yaml
version: number # ...
```

### Example

```yaml
version: 1.0
```

## Azure

### Schema

```yaml
azure:
  region: string                  # default region for project resource groups and resources
  subscriptionId: string          # subscription id where this TeamCloud instance is deployed
  servicePricipal:
    id: string                    # ...
    appId: string                 # ...
    secret: string                # ...
  subscriptionPoolIds: [ string ] # subscription ids to deploy project resources
  projectsPerSubscription: number # max number of projects per subscription
  resourceGroupNamePrefix: string # value to prepended to project resource group names
```

### Example

```yaml
azure:
  region: eastus
  subscriptionId: 96a2b11b-54d3-40a6-81ea-42ca77752af3
  servicePricipal:
    id: 3127c4c9-bf2d-4453-bb4a-df7fb2390bc5
    appId: 3127c4c9-bf2d-4453-bb4a-df7fb2390bc5
    secret: eb17e3ed-562f-4209-a99c-77cebf1f72d5
  subscriptionPoolIds:
  - b54e1d81-8c0c-4442-9283-9869630d060a
  - aba7458f-746e-4a59-a3f0-ebf36eddfcf6
  - 13f34552-b3d0-4099-a672-a8ac7a830db2
  projectsPerSubscription: 5
  resourceGroupNamePrefix: tc_
```

## Providers

### Schema

```yaml
providers:
- id: string           # unique identifier of the Provider (a-z and period)
  location: string     # URL where the Provider is hosted, used by the orchestrator to call the Provider
  authCode: string     # authorization code required to securely invoke the Provider service
  optional: boolean    # defaults to true; whether this provider will be included in every Project upon creation
  dependencies:
    create: [ string ] # ids of Providers that this Provider's Init call is dependent on
    init: [ string ]   # ids of Providers that this Provider's Create call is dependent on
  events: [ string ]   # ids of Providers that this Provider should recieve events for
  variables: { string: string }
```

### Example

```yaml
providers:
- id: azure.devtestlab
  location: 'https://github.com/Azure/azure-sdk-for-ios/tree/master/AzureData'
  authKey: foobar
  optional: false
  dependencies:
    create:
    - azure.applicationinsights
  variables:
    dtlvar1: dtlvalue1
    dtlvar2: dtlvalue2
  events:
  - azure.devtestlab
- id: azure.applicationinsights
  location: 'https://github.com/Azure/azure-sdk-for-ios/tree/master/AzureData'
  authKey: barfoo
  optional: false
  dependencies:
    init:
    - azure.devtestlab
  events:
  - azure.devtestlab
  - azure.applicationinsights
  variables:
    aivar1: aivalue1
    aivar2: aivalue2
```

## Users

### Schema

```yaml
providers:
- id: guid                  # unique identifier of the User (active director objectId)
  role: strings             # the role of the user (Admin or Creator)
  tags: { string: string }  # ...
```

### Example

```yaml
users:
- id: bc8a62dc-c327-4418-a004-77c85c3fb488
  role: Admin
  tags:
    usertag1: tagvalue1
    usertag2: tagvalue2
- id: aba7458f-746e-4a59-a3f0-ebf36eddfcf6
  role: Creator
```

## Tags

### Schema

```yaml
tags: { string: string } # ...
```

### Example

```yaml
tags:
  tag1: tagvalue1
  tag2: tagvalue2
  tag3: tagvalue3
```

## Variables

### Schema

```yaml
variables: { string: string } # ...
```

### Example

```yaml
variables:
  var1: value1
  var2: value2
  var3: value3
```

## Complete Example

```yaml
---
version: 1.0
azure:
  region: eastus
  subscriptionId: 96a2b11b-54d3-40a6-81ea-42ca77752af3
  servicePricipal:
    id: 3127c4c9-bf2d-4453-bb4a-df7fb2390bc5
    appId: 3127c4c9-bf2d-4453-bb4a-df7fb2390bc5
    secret: eb17e3ed-562f-4209-a99c-77cebf1f72d5
  subscriptionPoolIds:
  - b54e1d81-8c0c-4442-9283-9869630d060a
  - aba7458f-746e-4a59-a3f0-ebf36eddfcf6
  - 13f34552-b3d0-4099-a672-a8ac7a830db2
  projectsPerSubscription: 5
  resourceGroupNamePrefix: tc_
providers:
- id: azure.devtestlab
  location: 'https://github.com/Azure/azure-sdk-for-ios/tree/master/AzureData'
  authKey: foobar
  optional: false
  dependencies:
    create:
    - azure.applicationinsights
  events:
  - azure.devtestlab
  variables:
    dtlvar1: dtlvalue1
    dtlvar2: dtlvalue2
- id: azure.applicationinsights
  location: 'https://github.com/Azure/azure-sdk-for-ios/tree/master/AzureData'
  authKey: barfoo
  optional: false
  dependencies:
    init:
    - azure.devtestlab
  events:
  - azure.devtestlab
  - azure.applicationinsights
  variables:
    aivar1: aivalue1
    aivar2: aivalue2
tags:
  tag1: tagvalue1
  tag2: tagvalue2
  tag3: tagvalue3
variables:
  var1: value1
  var2: value2
  var3: value3
...
```

[regions-supported]:https://azure.microsoft.com/en-us/global-infrastructure/services/?products=api-management,functions,storage,key-vault,app-configuration,monitor,azure-devops,devtest-lab&regions=all