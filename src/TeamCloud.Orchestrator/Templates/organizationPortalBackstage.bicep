param resourceName string

param organizationName string
param organizationSlug string
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

var portalImage = 'teamcloud/tcportal-backstage:latest'

var databaseUsername = 'portal'  
var databasePassword = '${guid(resourceGroup().id)}'

resource portalPostgreSql 'Microsoft.DBforPostgreSQL/flexibleServers@2021-06-01' =  {
  name: resourceName
  location: location
  tags: organizationTags
  sku: {
    name: 'Standard_D4s_v3'
    tier: 'GeneralPurpose'
  }
  properties:{
    administratorLogin: databaseUsername
    administratorLoginPassword: databasePassword
    version: '13'
    storage: {
        storageSizeGB: 32
    }
    backup: {
        backupRetentionDays: 7
        geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
        mode: 'Disabled'
    }  
  }

  resource allowAllWindowsAzureIps 'firewallRules' = {
    name: 'AllowAllWindowsAzureIps' 
    properties: {
      endIpAddress: '0.0.0.0'
      startIpAddress: '0.0.0.0'
    }
  }
}

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

resource portalAppService 'microsoft.web/sites@2021-03-01' =  {
  name: resourceName
  location: location
  tags: organizationTags
  properties: {
    serverFarmId: portalAppServicePlan.id
    siteConfig: {
      appCommandLine: 'node packages/backend --config app-config.yaml --config app-config.production.yaml'
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
          value: '7007'
        }
        {
          name: 'POSTGRES_HOST'
          value: portalPostgreSql.properties.fullyQualifiedDomainName
        }
        {
          name: 'POSTGRES_PORT'
          value: '5432' 
        }
        {
          name: 'POSTGRES_USER'
          value: databaseUsername
        }
        {
          name: 'POSTGRES_PASSWORD'
          value: databasePassword
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
          name: 'AUTH_MICROSOFT_CLIENT_ID'
          value: portalClientId
        }
        {
          name: 'AUTH_MICROSOFT_CLIENT_SECRET'
          value: portalClientSecret
        }
        {
          name: 'AUTH_MICROSOFT_TENANT_ID'
          value: portalTenantId
        }
      ]
    }
  }
}

resource portalPublishing 'microsoft.web/sites/config@2021-03-01' existing = {
  name: '${resourceName}/publishingCredentials'
}

output portalUrl string = 'https://${portalAppService.properties.defaultHostName}'
output portalReplyUrl string = 'https://${portalAppService.properties.defaultHostName}/api/auth/microsoft/handler/frame'
output portalUpdateUrl string = 'https://${list(portalPublishing.id, '2021-03-01').properties.scmUri}/docker/hook'
