param tags object = {}
param deploymentScopes array = []

var resourcePrefix = 'tc'
var uniqueName = '${resourcePrefix}${uniqueString(resourceGroup().id)}'

resource projectSharedVault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: '${uniqueName}-shared'
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

resource projectSecretsVault 'Microsoft.KeyVault/vaults@2019-09-01' = {
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

resource projectStorageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: uniqueName
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {}
  tags: tags
}

resource projectIdentities 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = [for item in deploymentScopes: if (!empty(deploymentScopes)) {
  name: item
  location: resourceGroup().location
}]

output projectData object = {
  resourceId: resourceGroup().id
  sharedVaultId: projectSharedVault.id
  secretsVaultId: projectSecretsVault.id
  storageId: projectStorageAccount.id
}
