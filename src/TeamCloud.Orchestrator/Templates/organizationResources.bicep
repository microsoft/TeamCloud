param organizationTags object = {}
param location string = resourceGroup().location

var resourcePrefix = 'tc'
var resourceName = '${resourcePrefix}${uniqueString(resourceGroup().id)}'

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

output organizationData object = {
  resourceId: resourceGroup().id
  secretsVaultId: organizationSecretsVault.id
  galleryId: organizationSharedImageGallery.id
  registryId: organizationContainerRegistry.id
  storageId: organizationStorageAccount.id
}
