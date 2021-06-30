# TeamCloud Client SDKs

This file contains the configuration for generating client SDKs for the TeamCloud API.

> see https://aka.ms/autorest

## Getting Started

To generate the client SDKs for the TeamCloud API, simply install AutoRest via `npm` (`[sudo] npm install -g autorest`) and then run:

```shell
cd path/to/TeamCloud/client
autorest --v3 python.md
```

For other options on installation see [Installing AutoRest](https://aka.ms/autorest/install) on the AutoRest GitHub page.

## Configuration

The remainder of this file is configuration details used by AutoRest.

### Inputs

``` yaml
use: '@autorest/python@https://github.com/Azure/autorest.python/releases/download/v5.8.0/autorest-python-5.8.0.tgz'
input-file: openapi.yaml
namespace: teamcloud
add-credentials: true
credential-scopes: openid
override-client-name: TeamCloudClient
license-header: MICROSOFT_MIT_NO_VERSION
output-folder: './../client/tc/azext_tc/vendored_sdks/teamcloud'
no-namespace-folders: true
clear-output-folder: true
```
