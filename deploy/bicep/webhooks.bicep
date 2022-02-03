param registryName string = 'TeamCloud'
param registryResourceGroupName string = 'TeamCloud-Registry'
param registrySubscriptionId string

param apiAppName string = 'teamclouddemo-api'
param orchestratorAppName string = 'teamclouddemo-orchestrator'
param webAppName string = 'teamclouddemo'

resource api 'Microsoft.Web/sites@2021-02-01' existing = {
  name: apiAppName
}

resource orchestrator 'Microsoft.Web/sites@2021-02-01' existing = {
  name: orchestratorAppName
}

resource website 'Microsoft.Web/sites@2021-02-01' existing = {
  name: webAppName
}

module api_webhook 'webhook.bicep' = {
  scope: resourceGroup(registrySubscriptionId, registryResourceGroupName)
  name: 'apiWebhook'
  params: {
    name: 'webapi'
    registryName: registryName
    repository: 'teamcloud/api'
    serviceUrl: '${list('${api.id}/config/publishingcredentials', api.apiVersion).properties.scmUri}/docker/hook'
  }
}

module orchestrator_webhook 'webhook.bicep' = {
  scope: resourceGroup(registrySubscriptionId, registryResourceGroupName)
  name: 'orchestratorWebhook'
  params: {
    name: 'orchestrator'
    registryName: registryName
    repository: 'teamcloud/orchestrator'
    serviceUrl: '${list('${orchestrator.id}/config/publishingcredentials', orchestrator.apiVersion).properties.scmUri}/docker/hook'
  }
}

module website_webhook 'webhook.bicep' = {
  scope: resourceGroup(registrySubscriptionId, registryResourceGroupName)
  name: 'websiteWebhook'
  params: {
    name: 'website'
    registryName: registryName
    repository: 'teamcloud/website'
    serviceUrl: '${list('${website.id}/config/publishingcredentials', website.apiVersion).properties.scmUri}/docker/hook'
  }
}
