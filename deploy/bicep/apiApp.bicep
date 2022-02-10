param name string
param webAppName string
param appConfigName string
param appInsightsName string
param teamcloudImageRepo string = 'teamcloud'

resource config 'Microsoft.AppConfiguration/configurationStores@2021-03-01-preview' existing = {
  name: appConfigName
}

resource ai 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource farm 'Microsoft.Web/serverfarms@2021-02-01' = {
  kind: 'api,linux'
  name: name
  location: resourceGroup().location
  properties: {
    reserved: true
  }
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
}

resource api 'Microsoft.Web/sites@2021-02-01' = {
  kind: 'api,linux,container'
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
      alwaysOn: true
      phpVersion: 'off'
      linuxFxVersion: 'DOCKER|teamcloud.azurecr.io/${teamcloudImageRepo}/api'
      // detailedErrorLoggingEnabled: true
      // httpLoggingEnabled: true
      cors: {
        allowedOrigins: [
          'http://localhost:3000'
          'https://${webAppName}.azurewebsites.net'
        ]
        supportCredentials: true
      }
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
          name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
          value: 'disabled'
        }
        {
          name: 'APPINSIGHTS_SNAPSHOTFEATURE_VERSION'
          value: 'disabled'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~2'
        }
        {
          name: 'DiagnosticServices_EXTENSION_VERSION'
          value: 'disabled'
        }
        {
          name: 'InstrumentationEngine_EXTENSION_VERSION'
          value: 'disabled'
        }
        {
          name: 'SnapshotDebugger_EXTENSION_VERSION'
          value: 'disabled'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_BaseExtensions'
          value: 'disabled'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'default'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '10.14'
        }
        {
          name: 'ANCM_ADDITIONAL_ERROR_PAGE_LINK'
          value: 'https://${name}.scm.azurewebsites.net/detectors'
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
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

// resource apiLogging 'Microsoft.Web/sites/config@2021-02-01' = {
//   name: 'logs'
//   parent: api
//   properties: {
//     applicationLogs: {
//       fileSystem: {
//         level: 'Warning'
//       }
//     }
//     detailedErrorMessages: {
//       enabled: true
//     }
//     httpLogs: {
//       fileSystem: {
//         enabled: true
//       }
//     }
//     failedRequestsTracing: {
//       enabled: true
//     }
//   }
// }

output name string = name
output url string = 'https://${name}.azurewebsites.net'
output principalId string = api.identity.principalId
output tenantId string = api.identity.tenantId
