<!-- # TeamCloud Client SDKs

This file contains the configuration for generating client SDKs for the TeamCloud API.

> see https://aka.ms/autorest

## Getting Started

To generate the client SDKs for the TeamCloud API, simply install AutoRest via `npm` (`[sudo] npm install -g autorest`) and then run:

```shell
cd path/to/TeamCloud/client
autorest --v3
```

For other options on installation see [Installing AutoRest](https://aka.ms/autorest/install) on the AutoRest GitHub page.

## Configuration

The remainder of this file is configuration details used by AutoRest.

### Inputs

``` yaml
input-file: swagger.json
namespace: teamcloud
add-credentials: true
override-client-name: TeamCloudClient
license-header: MICROSOFT_MIT_NO_VERSION
```

### Generation

#### CSharp

``` yaml
# Uncomment to generate CSharp client.
# csharp:
#   output-folder: CSharp
#   namespace: TeamCloud.Client
```

#### NodeJS

``` yaml
# Uncomment to generate NodeJS client.
# nodejs:
#   output-folder: NodeJS
```

#### Python

``` yaml
python:
#   # output-folder: Python
  use-extension:
    "@autorest/python": "5.1.0-preview.7"
  no-namespace-folders: true
  output-folder: tc/azext_tc/vendored_sdks/teamcloud
``` -->
