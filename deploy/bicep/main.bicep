@description('The name of the TeamCloud instance that you wish to create. This will also be used as the subdomain of your service endpoint (i.e. myteamcloud.azurewebsites.net).')
param webAppName string

@description('The ClientId of the service principals used to authenticate users and create new Resource Groups for Projecs.')
param resourceManagerIdentityClientId string

@description('The ClientSecret of the service principals used to authenticate users and create new Resource Groups for Projecs.')
param resourceManagerIdentityClientSecret string

var name = toLower(webAppName)
var suffix = uniqueString(resourceGroup().id)
var functionAppName = '${name}-orchestrator'
var functionAppRoleAssignmentId = guid('${resourceGroup().id}${functionAppName}contributor')
var contributorRoleDefinitionId = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/b24988ac-6180-42a0-ab88-20f7382dd24c'

module cosmos 'cosmosDb.bicep' = {
  name: 'cosmosDb'
  params: {
    name: 'database${suffix}'
  }
}

// module redis 'redis.bicep' = {
//   name: 'redis'
//   params: {
//     name: 'redis${suffix}'
//   }
// }

module kv 'keyVault.bicep' = {
  name: 'keyVault'
  params: {
    name: 'keyvault${suffix}'
  }
}

module config 'appConfig.bicep' = {
  name: 'appConfig'
  params: {
    name: '${name}-config'
  }
}

module storage_wj 'storage.bicep' = {
  name: 'webjobsStorage'
  params: {
    name: 'wjstorage${suffix}'
  }
}

module storage_th 'storage.bicep' = {
  name: 'taskhubStorage'
  params: {
    name: 'thstorage${suffix}'
  }
}

module storage_dep 'storage.bicep' = {
  name: 'deploymentStorage'
  params: {
    name: 'depstorage${suffix}'
  }
}

module ai 'appInsights.bicep' = {
  name: 'appInsights'
  params: {
    name: name
  }
}

module api 'webApp.bicep' = {
  name: 'api'
  params: {
    name: name
    appInsightsInstrumentationKey: ai.outputs.instrumentationKey
    appConfigurationConnectionString: config.outputs.connectionString
  }
}

module apiPolicy 'keyVaultPolicy.bicep' = {
  name: 'apiPolicy'
  params: {
    keyVaultName: kv.outputs.name
    principalId: api.outputs.principalId
    tenantId: api.outputs.tenantId
  }
}

module orchestrator 'functionApp.bicep' = {
  name: 'orchestrator'
  params: {
    name: functionAppName
    appInsightsInstrumentationKey: ai.outputs.instrumentationKey
    webjobsStorageConnectionString: storage_wj.outputs.connectionString
    taskhubStorageConnectionString: storage_th.outputs.connectionString
    appConfigurationConnectionString: config.outputs.connectionString
  }
}

module funcRole 'roleAssignment.bicep' = {
  name: 'orchestratorRole'
  params: {
    name: functionAppRoleAssignmentId
    principalId: orchestrator.outputs.principalId
    roleDefinitionId: contributorRoleDefinitionId
  }
}

module funcPolicy 'keyVaultPolicy.bicep' = {
  name: 'orchestratorPolicy'
  params: {
    keyVaultName: kv.outputs.name
    principalId: orchestrator.outputs.principalId
    tenantId: orchestrator.outputs.tenantId
  }
}

module signalr 'signalR.bicep' = {
  name: 'signalR'
  params: {
    name: name
  }
}

output apiUrl string = api.outputs.url
output apiAppName string = api.outputs.name
output orchestratorUrl string = orchestrator.outputs.url
output orchestratorAppName string = orchestrator.outputs.name
output configServiceConnectionString string = config.outputs.connectionString
output configServiceImport object = {
  'Azure:TenantId': subscription().tenantId
  'Azure:SubscriptionId': subscription().subscriptionId
  'Azure:ResourceManager:ClientId': resourceManagerIdentityClientId
  'Azure:ResourceManager:ClientSecret': resourceManagerIdentityClientSecret
  'Azure:ResourceManager:TenantId': subscription().tenantId
  'Azure:CosmosDb:TenantName': 'TeamCloud'
  'Azure:CosmosDb:DatabaseName': 'TeamCloud'
  'Azure:CosmosDb:ConnectionString': cosmos.outputs.connectionString
  'Azure:DeploymentStorage:ConnectionString': storage_dep.outputs.connectionString
  'Azure:SignalR:ConnectionString': signalr.outputs.connectionString
  'Endpoint:Api:Url': api.outputs.url
  'Endpoint:Orchestrator:Url': orchestrator.outputs.url
  'Endpoint:Orchestrator:AuthCode': orchestrator.outputs.key
  // 'Cache:Configuration': redis.outputs.configuration
  'Encryption:KeyStorage': storage_wj.outputs.connectionString
  'Audit:ConnectionString': storage_wj.outputs.connectionString
  'Adapter:Session:Storage:ConnectoinString': storage_wj.outputs.connectionString
  'Adapter:Token:Storage:ConnectionString': storage_wj.outputs.connectionString
}
