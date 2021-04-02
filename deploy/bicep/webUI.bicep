@description('The name of the web app that you wish to create. This will also be used as the subdomain of your app endpoint (i.e. myapp.azurewebsites.net).')
param webAppName string

@description('The ClientId of the Managed Application used to authenticate users. See https://aka.ms/tcwebclientid for details.')
param reactAppMsalClientId string

@description('Base url of the TeamCloud instance.')
param reactAppTcApiUrl string

@description('Scope.')
param reactAppMsalScope string = 'http://TeamCloud.Web/user_impersonation'

var name = toLower(webAppName)

resource farm 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: name
  kind: 'linux'
  location: resourceGroup().location
  properties: {
    perSiteScaling: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: true
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
  }
  sku: {
    name: 'P1v2'
    tier: 'PremiumV2'
    size: 'P1v2'
    family: 'Pv2'
    capacity: 1
  }
}

resource app 'Microsoft.Web/sites@2020-06-01' = {
  name: name
  kind: 'app,linux'
  location: resourceGroup().location
  properties: {
    reserved: true
    serverFarmId: farm.id
    clientAffinityEnabled: false
    siteConfig: {
      alwaysOn: true
      linuxFxVersion: 'NODE|12-lts'
      appCommandLine: 'pm2 serve /home/site/wwwroot --no-daemon --spa'
      phpVersion: 'off'
      appSettings: [
        {
          name: 'REACT_APP_MSAL_CLIENT_ID'
          value: reactAppMsalClientId
        }
        {
          name: 'REACT_APP_MSAL_TENANT_ID'
          value: subscription().tenantId
        }
        {
          name: 'REACT_APP_MSAL_SCOPE'
          value: reactAppMsalScope
        }
        {
          name: 'REACT_APP_TC_API_URL'
          value: reactAppTcApiUrl
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '12'
        }
        {
          name: 'WEBSITE_NPM_DEFAULT_VERSION'
          value: '6'
        }
      ]
    }
  }
}
