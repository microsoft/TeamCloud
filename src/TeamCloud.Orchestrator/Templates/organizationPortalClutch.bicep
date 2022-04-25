param resourceName string

param organizationName string
// param organizationSlug string
param organizationTags object = {}
param location string = resourceGroup().location

param portalClientId string
param portalTenantId string = tenant().tenantId

@secure()
param portalClientSecret string

param registryServer string = 'teamcloud.azurecr.io'
param registryUsername string = ''

@secure()
param registryPassword string = ''

param storageAccountName string

@secure()
param storageAcountKey string

var portalImage = 'teamcloud/tcportal-clutch:latest'

resource portalAppServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: resourceName
  location: location
  tags: organizationTags
  kind: 'linux'
  sku: {
    name: 'S2'
    tier: 'Standard'
  }
  properties: {
    targetWorkerSizeId: 0
    targetWorkerCount: 1
    reserved: true
  }
}

resource portalAppService 'microsoft.web/sites@2021-03-01' = {
  name: resourceName
  location: location
  tags: organizationTags
  properties: {
    serverFarmId: portalAppServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${registryServer}/${portalImage}'
      appSettings: [
        {
          name: 'DOCKER_ENABLE_CI'
          value: 'true'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${registryServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: registryUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: registryPassword
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'TEAMCLOUD_ORGANIZATION_NAME'
          value: organizationName
        }
        {
          name: 'STORAGE_ACCOUNT_NAME'
          value: storageAccountName
        }
        {
          name: 'STORAGE_ACCOUNT_KEY'
          value: storageAcountKey
        }
        {
          name: 'CREDENTIALS_OIDC_CLIENT_ID'
          value: portalClientId
        }
        {
          name: 'CREDENTIALS_OIDC_CLIENT_SECRET'
          value: portalClientSecret
        }
        {
          name: 'CREDENTIALS_OIDC_TENANT_ID'
          value: portalTenantId
        }
        {
          name: 'CREDENTIALS_SESSION_SECRET'
          value: guid(resourceGroup().id)
        }
      ]
    }
  }
}

resource portalPublishing 'microsoft.web/sites/config@2021-03-01' existing = {
  name: '${resourceName}/publishingCredentials'
}

output portalId string = portalAppService.id
output portalUrl string = 'https://${portalAppService.properties.defaultHostName}'
output portalReplyUrl string = 'https://${portalAppService.properties.defaultHostName}/v1/authn/callback'
#disable-next-line outputs-should-not-contain-secrets
output portalUpdateUrl string = '${list(portalPublishing.id, '2021-03-01').properties.scmUri}/docker/hook'
