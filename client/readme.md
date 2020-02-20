# TeamCloud Client SDKs

This file contains the configuration for generating client SDKs for the TeamCloud API.

> see https://aka.ms/autorest

## Getting Started

To generate the client SDKs for the TeamCloud API:

### 1. Install AutoRest

Normally, you'd simply install AutoRest via `npm` (`npm install -g autorest`). However, TeamCloud API is based on OpenAPI v3.0.1, which requires AutoRest v3 to generate the SDKs. AutoRest v3 is [currently in preview](https://github.com/Azure/autorest/tree/v3#new-autorest-version-30). Prerelease builds can be installed directly from a GitHub release (using the command below).

```shell
$ (sudo) npm install -g  https://github.com/Azure/autorest/releases/download/autorest-3.0.6182/autorest-3.0.6182.tgz
$ autorest --reset
```

The most recent version of the TeamCloud Client SDKs were generated using prerelease build [3.0.6182](https://github.com/Azure/autorest/releases/tag/autorest-3.0.6182).

This is only necessary until the AutoRest v3 is promoted to GA (soon). More info on managing AutoRest versions can be found [here](https://github.com/Azure/autorest/blob/v3/docs/autorest-versioning.md).

### 2. Run AutoRest

Next step is to run `autorest` in this (client) folder:

```shell
$ cd path/to/TeamCloud/client
$ autorest
```

## Configuration

The remainder of this file is configuration details used by AutoRest.

### Inputs

``` yaml
input-file: swagger.json
namespace: teamcloud
add-credentials: true
override-client-name: TeamCloudClient
license-header: MICROSOFT_MIT_NO_CODEGEN
# license-header: MICROSOFT_MIT_NO_VERSION
```

### Generation

#### CSharp

``` yaml
csharp:
  output-folder: CSharp
  namespace: TeamCloud.Client
```

#### NodeJS

``` yaml
nodejs:
  output-folder: NodeJS
```

#### Python

``` yaml
python:
  output-folder: Python
```
