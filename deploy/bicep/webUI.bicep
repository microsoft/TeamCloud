@description('The name of the web app that you wish to create. This will also be used as the subdomain of your app endpoint (i.e. myapp.azurewebsites.net).')
param webAppName string

@description('The ClientId of the Managed Application used to authenticate users. See https://aka.ms/tcwebclientid for details.')
param reactAppMsalClientId string

@description('Base url of the TeamCloud instance.')
param reactAppTcApiUrl string

@description('Scope.')
param reactAppMsalScope string = 'http://TeamCloud.Web/user_impersonation'

param reactAppVersion string = ''

var name = toLower(webAppName)

resource farm 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: name
  kind: 'app,linux'
  location: resourceGroup().location
  properties: {
    reserved: true
  }
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
}

resource app 'Microsoft.Web/sites@2021-02-01' = {
  name: name
  kind: 'app'
  location: resourceGroup().location
  properties: {
    reserved: true
    serverFarmId: farm.id
    clientAffinityEnabled: false
    siteConfig: {
      alwaysOn: true
      phpVersion: 'off'
      linuxFxVersion: 'NODE|14-lts'
      appCommandLine: 'pm2 serve /home/site/wwwroot --no-daemon --spa'
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
          name: 'REACT_APP_VERSION'
          value: reactAppVersion
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '16'
        }
        {
          name: 'WEBSITE_NPM_DEFAULT_VERSION'
          value: '6'
        }
      ]
    }
  }
}

output name string = name
output url string = 'https://${name}.azurewebsites.net'
