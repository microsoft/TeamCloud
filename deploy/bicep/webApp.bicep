param name string
@secure()
param appInsightsInstrumentationKey string
@secure()
param appConfigurationConnectionString string

resource farm 'Microsoft.Web/serverfarms@2020-06-01' = {
  kind: 'app'
  name: name
  location: resourceGroup().location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  properties: {
    perSiteScaling: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: false
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
  }
}

resource web 'Microsoft.Web/sites@2020-06-01' = {
  kind: 'app'
  name: name
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: farm.id
    clientAffinityEnabled: false
    siteConfig: {
      cors: {
        allowedOrigins: [
          'http://localhost:3000'
          'https://${name}-web.azurewebsites.net'
        ]
        supportCredentials: true
      }
      phpVersion: 'off'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnetcore'
        }
      ]
      appSettings: [
        {
          name: 'AppConfiguration:ConnectionString'
          value: appConfigurationConnectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
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
output principalId string = web.identity.principalId
output tenantId string = web.identity.tenantId
