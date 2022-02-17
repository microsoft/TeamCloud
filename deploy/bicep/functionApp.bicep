param location string = resourceGroup().location
param name string
param appConfigName string
param appInsightsName string
param webjobStorageName string
param taskhubStorageName string
param teamcloudImageRepo string = 'teamcloud'
param clientId string
@secure()
param clientSecret string

resource config 'Microsoft.AppConfiguration/configurationStores@2021-03-01-preview' existing = {
  name: appConfigName
}

resource ai 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource th_storage 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: taskhubStorageName
}

resource wj_storage 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: webjobStorageName
}

resource farm 'Microsoft.Web/serverfarms@2021-02-01' = {
  kind: 'functionapp,linux'
  name: name
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'EP1'
    tier: 'ElasticPremium'
  }
}

resource func 'Microsoft.Web/sites@2021-02-01' = {
  kind: 'functionapp,linux,container'
  name: name
  location: location
  properties: {
    reserved: true
    serverFarmId: farm.id
    clientAffinityEnabled: false
    siteConfig: {
      phpVersion: 'off'
      linuxFxVersion: 'DOCKER|teamcloud.azurecr.io/${teamcloudImageRepo}/orchestrator'
      use32BitWorkerProcess: false
      appSettings: [
        {
          name: 'AZURE_CLIENT_ID'
          value: clientId
        }
        {
          name: 'AZURE_TENANT_ID'
          value: subscription().tenantId
        }
        {
          name: 'AZURE_CLIENT_SECRET'
          value: clientSecret
        }
        {
          name: 'AppConfiguration__ConnectionString'
          value: listKeys(config.id, '2019-10-01').value[0].connectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: ai.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: ai.properties.ConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${webjobStorageName};AccountKey=${wj_storage.listKeys().keys[0].value}'
        }
        {
          name: 'DurableFunctionsHubStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${taskhubStorageName};AccountKey=${th_storage.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${webjobStorageName};AccountKey=${wj_storage.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: name
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://teamcloud.azurecr.io'
        }
        {
          name: 'DOCKER_CUSTOM_IMAGE_NAME'
          value: 'teamcloud.azurecr.io/${teamcloudImageRepo}/orchestrator'
        }
        {
          name: 'FUNCTION_APP_EDIT_MODE'
          value: 'readOnly'
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
      ]
      connectionStrings: [
        {
          name: 'ConfigurationService'
          connectionString: listKeys(config.id, '2019-10-01').value[0].connectionString
          type: 'Custom'
        }
      ]
    }
  }
}

output name string = name
output url string = 'https://${name}.azurewebsites.net'
