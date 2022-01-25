# TeamCloud Client SDKs

This file contains the configuration for generating client SDKs for the TeamCloud API.

> see https://aka.ms/autorest

## Getting Started

To generate the client SDKs for the TeamCloud API, simply install AutoRest via `npm` (`[sudo] npm install -g autorest`) and then run:

```shell
cd path/to/TeamCloud/client
autorest --v3 typescript.md
```

For other options on installation see [Installing AutoRest](https://aka.ms/autorest/install) on the AutoRest GitHub page.

## Configuration

The remainder of this file is configuration details used by AutoRest.

### Inputs

``` yaml
use: '@autorest/typescript@latest'
package-name: teamcloud
input-file: openapi.yaml
add-credentials: true
credential-scopes: openid
license-header: MICROSOFT_MIT_NO_VERSION
output-folder: './../web/teamcloud'
enum-types: true
clear-output-folder: true
```
