@description('The name of the TeamCloud instance that you wish to create. This will also be used as the subdomain of your service endpoint (i.e. myteamcloud.azurewebsites.net).')
param webAppName string

@description('The ClientId of the service principals used to authenticate users and create new Resource Groups for Projecs.')
param resourceManagerIdentityClientId string

@description('The ClientSecret of the service principals used to authenticate users and create new Resource Groups for Projecs.')
param resourceManagerIdentityClientSecret string

@description('The ClientId of the Managed Application used to authenticate users. See https://aka.ms/tcwebclientid for details.')
param reactAppMsalClientId string

@description('Scope.')
param reactAppMsalScope string = 'http://TeamCloud.Web/user_impersonation'

param reactAppVersion string = ''

param doSleepHack bool = false

var name = toLower(webAppName)
var webName = '${name}-web'
var suffix = uniqueString(resourceGroup().id)
var functionAppName = '${name}-orchestrator'
var functionAppRoleAssignmentId = guid('${resourceGroup().id}${functionAppName}contributor')
var contributorRoleDefinitionId = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/b24988ac-6180-42a0-ab88-20f7382dd24c'

module cosmos 'cosmosDb.bicep' = {
  name: 'cosmosDb'
  params: {
    name: 'database${suffix}'
    appConfigName: config.outputs.name
  }
}

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

module storage_dep 'storage.bicep' = {
  name: 'deploymentStorage'
  params: {
    name: 'depstorage${suffix}'
    appConfigName: config.outputs.name
    appConfigConnectionStringKeys: [
      'Azure:DeploymentStorage:ConnectionString'
    ]
  }
}

module storage_th 'storage.bicep' = {
  name: 'taskhubStorage'
  params: {
    name: 'thstorage${suffix}'
  }
}

module storage_wj 'storage.bicep' = {
  name: 'webjobsStorage'
  params: {
    name: 'wjstorage${suffix}'
    appConfigName: config.outputs.name
    appConfigConnectionStringKeys: [
      'Encryption:KeyStorage'
      'Audit:ConnectionString'
    ]
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
    appConfigName: config.outputs.name
    appInsightsName: ai.outputs.name
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
    appConfigName: config.outputs.name
    appInsightsName: ai.outputs.name
    taskhubStorageName: storage_th.outputs.name
    webjobStorageName: storage_wj.outputs.name
  }
}

module orchestratorRole 'roleAssignment.bicep' = {
  name: 'orchestratorRole'
  params: {
    name: functionAppRoleAssignmentId
    principalId: orchestrator.outputs.principalId
    roleDefinitionId: contributorRoleDefinitionId
  }
}

module orchestratorPolicy 'keyVaultPolicy.bicep' = {
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
    appConfigName: config.outputs.name
  }
}

module web 'webUI.bicep' = {
  name: 'webUI'
  params: {
    reactAppMsalClientId: reactAppMsalClientId
    reactAppMsalScope: reactAppMsalScope
    reactAppTcApiUrl: api.outputs.url
    reactAppVersion: reactAppVersion
    webAppName: webName
  }
}

module orchestratorKey 'functionKey.bicep' = {
  name: 'orchestratorKey'
  params: {
    functionAppName: orchestrator.outputs.name
    appConfigName: config.outputs.name
  }
  dependsOn: [
    sleepHack
    orchestrator
    orchestratorPolicy
    orchestratorRole
    signalr
  ]
}

// this is a hack to sleep for a couple of minutes, otherwise
// the function host runtime isn't ready to create a new host key
// and fails with InternalServerError
module sleepHack 'sleepHack.bicep' = if (doSleepHack) {
  name: 'sleepHack'
  params: {
    time: '5m'
  }
  dependsOn: [
    orchestrator
    orchestratorPolicy
    orchestratorRole
  ]
}

module commonConfigs 'appConfigKeys.bicep' = {
  name: 'commonConfigs'
  params: {
    configName: config.outputs.name
    keyValues: {
      'Azure:TenantId': subscription().tenantId
      'Azure:SubscriptionId': subscription().subscriptionId
      'Azure:ResourceManager:ClientId': resourceManagerIdentityClientId
      'Azure:ResourceManager:ClientSecret': resourceManagerIdentityClientSecret
      'Azure:ResourceManager:TenantId': subscription().tenantId
    }
  }
}

output apiUrl string = api.outputs.url
output apiAppName string = api.outputs.name
output webUrl string = web.outputs.url
output webAppName string = web.outputs.name
output orchestratorUrl string = orchestrator.outputs.url
output orchestratorAppName string = orchestrator.outputs.name
