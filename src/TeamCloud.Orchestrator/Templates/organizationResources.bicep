param tags object = {}

var resourcePrefix = 'tc'
var uniqueName = '${resourcePrefix}${uniqueString(resourceGroup().id)}'

resource organizationSecretsVault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: '${uniqueName}-secrets'
  location: resourceGroup().location
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
  tags: tags
}

resource organizationSharedImageGallery 'Microsoft.Compute/galleries@2020-09-30' = {
  name: uniqueName
  location: resourceGroup().location
  properties: {}
  tags: tags
}

resource organizationContainerRegistry 'Microsoft.ContainerRegistry/registries@2019-12-01-preview' = {
  name: uniqueName
  location: resourceGroup().location
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: false
  }
  tags: tags
}

resource organizationStorageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: uniqueName
  location: resourceGroup().location
  kind: 'StorageV2'
  sku: {
    name: 'Premium_LRS'
  }
  tags: tags
}

output organizationData object = {
  resourceId: resourceGroup().id
  secretsVaultId: organizationSecretsVault.id
  galleryId: organizationSharedImageGallery.id
  registryId: organizationContainerRegistry.id
  storageId: organizationStorageAccount.id
}
