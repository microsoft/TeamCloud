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
use: '@microsoft.azure/autorest.typescript@https://github.com/Azure/autorest.typescript/releases/download/v6.0.0-dev.20201013.1/autorest-typescript-6.0.0-dev.20201013.1.tgz'
package-name: teamcloud
input-file: swagger.yaml
add-credentials: true
credential-scopes: openid
license-header: MICROSOFT_MIT_NO_VERSION
output-folder: './../web/teamcloud/lib/teamcloud'
clear-output-folder: true

declare-directive:
  rename-component: >-
    [{
      from: 'swagger.yaml',
      where: '$.components.schemas',
      transform: `if ($[${JSON.stringify($.from)}]) { $[${JSON.stringify($.to)}] = $[${JSON.stringify($.from)}]; delete $[${JSON.stringify($.from)}]; }`
    },
    {
      from: 'swagger.yaml',
      where: `$..['$ref']`,
      transform: `$ = $ === "#/components/schemas/${$.from}" ? "#/components/schemas/${$.to}" : $`
    },
    {
      from: 'swagger.yaml',
      where: `$..['$ref']`,
      transform: `$ = $ === ($documentPath + "#/components/schemas/${$.from}") ? ($documentPath + "#/components/schemas/${$.to}") : $`
    }]

directive:
  - from: swagger.yaml
    where: $.components.schemas.ProviderData.properties.value
    transform: return undefined

  - rename-component:
      from: ProviderDataDataResult
      to: ProviderDataReturnResult
```
