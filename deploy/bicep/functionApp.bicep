param name string
@secure()
param appInsightsInstrumentationKey string
@secure()
param webjobsStorageConnectionString string
@secure()
param taskhubStorageConnectionString string
@secure()
param appConfigurationConnectionString string

resource farm 'Microsoft.Web/serverfarms@2020-06-01' = {
  kind: 'functionapp'
  name: name
  location: resourceGroup().location
  sku: {
    name: 'EP1'
    tier: 'ElasticPremium'
  }
}

resource func 'Microsoft.Web/sites@2020-06-01' = {
  kind: 'functionapp'
  name: name
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: farm.id
    clientAffinityEnabled: false
    siteConfig: {
      phpVersion: 'off'
      appSettings: [
        {
          name: 'AppConfiguration:ConnectionString'
          value: appConfigurationConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: webjobsStorageConnectionString
        }
        {
          name: 'DurableFunctionsHubStorage'
          value: taskhubStorageConnectionString
        }
        {
          name: 'FUNCTION_APP_EDIT_MODE'
          value: 'readonly'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: webjobsStorageConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: name
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~12'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
      ]
      connectionStrings: [
        {
          name: 'ConfigurationService'
          connectionString: appConfigurationConnectionString
          type: 'Custom'
        }
      ]
    }
  }
}

output name string = name
output url string = 'https://${name}.azurewebsites.net'
output key string = listkeys('${func.id}/host/default/', '2016-08-01').functionKeys.default
output principalId string = func.identity.principalId
output tenantId string = func.identity.tenantId
