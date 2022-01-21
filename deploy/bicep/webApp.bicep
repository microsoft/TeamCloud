param name string
param appConfigName string
param appInsightsName string

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

resource web 'Microsoft.Web/sites@2021-02-01' = {
  kind: 'api'
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
      linuxFxVersion: 'DOTNETCORE|6.0'
      cors: {
        allowedOrigins: [
          'http://localhost:3000'
          'https://${name}-web.azurewebsites.net'
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
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
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

module apiConfigs 'appConfigKeys.bicep' = {
  name: 'apiConfigs'
  params: {
    configName: appConfigName
    keyValues: {
      'Endpoint:Api:Url': 'https://${name}.azurewebsites.net'
    }
  }
}

output name string = name
output url string = 'https://${name}.azurewebsites.net'
output principalId string = web.identity.principalId
output tenantId string = web.identity.tenantId
