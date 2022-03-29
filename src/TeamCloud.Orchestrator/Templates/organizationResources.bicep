param organizationName string
param organizationSlug string
param organizationTags object = {}
param location string = resourceGroup().location

param portal string = 'teamcloud'
param portalClientId string = ''
param portalTenantId string = subscription().tenantId

@secure()
param portalClientSecret string = ''

var resourcePrefix = 'tc'
var resourceName = '${resourcePrefix}${uniqueString(resourceGroup().id)}'

var backstageEnabled = (portal =~ 'backstage')
var clutchEnabled = (portal =~ 'clutch')

var portalEnabled = (backstageEnabled || clutchEnabled)
var portalDeployment = take(deployment().name, lastIndexOf(deployment().name, '-'))

resource organizationSecretsVault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: '${resourceName}-secrets'
  location: location
  tags: organizationTags
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    enabledForDeployment: true
    enabledForDiskEncryption: true
    enabledForTemplateDeployment: true
    enableSoftDelete: true
    tenantId: subscription().tenantId
    accessPolicies: []
  }
}

resource organizationSharedImageGallery 'Microsoft.Compute/galleries@2020-09-30' = {
  name: resourceName
  location: location
  tags: organizationTags
  properties: {}
}

resource organizationContainerRegistry 'Microsoft.ContainerRegistry/registries@2019-12-01-preview' = {
  name: resourceName
  location: location
  tags: organizationTags
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: false
  }
}

resource organizationStorageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: resourceName
  location: location
  tags: organizationTags
  kind: 'StorageV2'
  sku: {
    name: 'Premium_LRS'
  }
}

module backstageDeployment './organizationPortalBackstage.bicep' = if (backstageEnabled) {
  name: '${portalDeployment}-backstage'
  params: {
    resourceName: resourceName
    organizationName: organizationName
    organizationSlug: organizationSlug
    organizationTags: organizationTags
    location: location
    portalClientId: portalClientId
    portalClientSecret: portalClientSecret
    portalTenantId: portalTenantId
    storageAccountName: organizationStorageAccount.name
    storageAcountKey: organizationStorageAccount.listKeys().keys[0].value 
  }
}

module clutchDeployment './organizationPortalClutch.bicep' = if (clutchEnabled) {
  name: '${portalDeployment}-clutch'
  params: {
    resourceName: resourceName
    organizationName: organizationName
    organizationSlug: organizationSlug
    organizationTags: organizationTags
    location: location
    portalClientId: portalClientId
    portalClientSecret: portalClientSecret
    portalTenantId: portalTenantId
    storageAccountName: organizationStorageAccount.name
    storageAcountKey: organizationStorageAccount.listKeys().keys[0].value 
  }
}

resource portalLogs 'microsoft.web/sites/config@2021-03-01' = if (portalEnabled) {
  name: '${resourceName}/logs'
  dependsOn: [
    backstageDeployment
    clutchDeployment
  ]
  properties: {
    applicationLogs: {
      fileSystem: {
        level: 'Warning'
      }
    }
    detailedErrorMessages: {
      enabled: true
    }
    failedRequestsTracing: {
      enabled: true
    }
    httpLogs: {
      fileSystem: {
        enabled: true
        retentionInDays: 1
        retentionInMb: 35
      }
    }
  }
}

output organizationData object = {
  resourceId: resourceGroup().id
  secretsVaultId: organizationSecretsVault.id
  galleryId: organizationSharedImageGallery.id
  registryId: organizationContainerRegistry.id
  storageId: organizationStorageAccount.id
  portalId: backstageEnabled ? backstageDeployment.outputs.portalId : clutchEnabled ? clutchDeployment.outputs.portalId : ''
  portalUrl: backstageEnabled ? backstageDeployment.outputs.portalUrl : clutchEnabled ? clutchDeployment.outputs.portalUrl : ''
  portalReplyUrl: backstageEnabled ? backstageDeployment.outputs.portalReplyUrl : clutchEnabled ? clutchDeployment.outputs.portalReplyUrl : ''
  portalUpdateUrl: backstageEnabled ? backstageDeployment.outputs.portalUpdateUrl : clutchEnabled ? clutchDeployment.outputs.portalUpdateUrl : ''
  portalIdentity: portalEnabled ? portalClientId : ''
}
