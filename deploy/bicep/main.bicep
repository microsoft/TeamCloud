@description('The name of the TeamCloud instance that you wish to create. This will also be used as the subdomain of your service endpoint (i.e. myteamcloud.azurewebsites.net).')
param webAppName string

@description('The ClientId of the service principals used to authenticate users and create new Resource Groups for Projecs.')
param resourceManagerIdentityClientId string

@description('The ClientSecret of the service principals used to authenticate users and create new Resource Groups for Projecs.')
param resourceManagerIdentityClientSecret string

param teamcloudImageRepo string = 'teamcloud'

@description('The ClientId of the Managed Application used to authenticate users. See https://aka.ms/tcwebclientid for details.')
param reactAppMsalClientId string

@description('Scope.')
param reactAppMsalScope string = 'http://TeamCloud.Web/user_impersonation'

param version string = ''

param doSleepHack bool = false

var name = toLower(webAppName)
var suffix = uniqueString(resourceGroup().id)
var apiAppName = '${name}-api'
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
      'Azure:Storage:ConnectionString'
    ]
  }
}

module ai 'appInsights.bicep' = {
  name: 'appInsights'
  params: {
    name: name
  }
}

module api 'apiApp.bicep' = {
  name: 'api'
  params: {
    name: apiAppName
    webAppName: name
    appConfigName: config.outputs.name
    appInsightsName: ai.outputs.name
    teamcloudImageRepo: teamcloudImageRepo
  }
  dependsOn: [
    orchestratorKey
  ]
}

module orchestrator 'functionApp.bicep' = {
  name: 'orchestrator'
  params: {
    name: functionAppName
    appConfigName: config.outputs.name
    appInsightsName: ai.outputs.name
    taskhubStorageName: storage_th.outputs.name
    webjobStorageName: storage_wj.outputs.name
    teamcloudImageRepo: teamcloudImageRepo
  }
  dependsOn: [
    cosmos
    signalr
    storage_dep
    kv
  ]
}

module apiPolicy 'keyVaultPolicy.bicep' = {
  name: 'apiPolicy'
  params: {
    keyVaultName: kv.outputs.name
    principalId: api.outputs.principalId
    tenantId: api.outputs.tenantId
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

module web 'website.bicep' = {
  name: 'website'
  params: {
    reactAppMsalClientId: reactAppMsalClientId
    reactAppMsalScope: reactAppMsalScope
    reactAppTcApiUrl: api.outputs.url
    webAppName: name
    teamcloudImageRepo: teamcloudImageRepo
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
  ]
}

// this is a hack to wait until the function app host key is available,
// otherwise, the function host runtime isn't ready to create a new host key
// and fails with InternalServerError
module sleepHack 'sleepHack.bicep' = if (doSleepHack) {
  name: 'sleepHack'
  params: {
    functionAppName: orchestrator.outputs.name
  }
}

module commonConfigs 'appConfigKeys.bicep' = {
  name: 'commonConfigs'
  params: {
    configName: config.outputs.name
    keyValues: {
      'TeamCloud:Version': version
      'Azure:TenantId': subscription().tenantId
      'Azure:SubscriptionId': subscription().subscriptionId
      'Azure:ResourceManager:ClientId': resourceManagerIdentityClientId
      'Azure:ResourceManager:ClientSecret': resourceManagerIdentityClientSecret
      'Azure:ResourceManager:TenantId': subscription().tenantId
      'Endpoint:Api:Url': 'https://${apiAppName}.azurewebsites.net'
      'Endpoint:Orchestrator:Url': 'https://${functionAppName}.azurewebsites.net'
    }
  }
}

output apiUrl string = api.outputs.url
output apiAppName string = api.outputs.name
output webUrl string = web.outputs.url
output webAppName string = web.outputs.name
output orchestratorUrl string = orchestrator.outputs.url
output orchestratorAppName string = orchestrator.outputs.name
