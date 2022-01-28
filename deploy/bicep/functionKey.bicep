param appConfigName string
param functionAppName string

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
