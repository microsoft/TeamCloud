// param keyName string
param functionAppName string
param appConfigName string

// #disable-next-line BCP081
// resource key 'Microsoft.Web/sites/host/functionKeys@2021-02-01' = {
//   name: '${functionAppName}/default/${keyName}'
//   properties: {
//     name: keyName
//   }
// }

resource func 'Microsoft.Web/sites@2021-02-01' existing = {
  name: functionAppName
}

module orchestratorKeyConfigs 'appConfigKeys.bicep' = {
  name: 'orchestratorKeyConfigs'
  params: {
    configName: appConfigName
    keyValues: {
      'Endpoint:Orchestrator:AuthCode': listkeys('${func.id}/host/default/', '2021-02-01').functionKeys.default
    }
  }
}
