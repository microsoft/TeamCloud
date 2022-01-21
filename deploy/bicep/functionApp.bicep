param name string
param appConfigName string
param appInsightsName string
param webjobStorageName string
param taskhubStorageName string

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
  location: resourceGroup().location
  properties: {
    reserved: true
  }
  sku: {
    name: 'EP1'
    tier: 'ElasticPremium'
  }
}

resource func 'Microsoft.Web/sites@2021-02-01' = {
  kind: 'functionapp'
  name: name
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    reserved: true
    serverFarmId: farm.id
    clientAffinityEnabled: false
    siteConfig: {
      phpVersion: 'off'
      linuxFxVersion: 'DOTNET|6.0'
      appSettings: [
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
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
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

module orchestratorConfigs 'appConfigKeys.bicep' = {
  name: 'orchestratorConfigs'
  params: {
    configName: appConfigName
    keyValues: {
      'Endpoint:Orchestrator:Url': 'https://${name}.azurewebsites.net'
    }
  }
}

output name string = name
output url string = 'https://${name}.azurewebsites.net'
output principalId string = func.identity.principalId
output tenantId string = func.identity.tenantId
