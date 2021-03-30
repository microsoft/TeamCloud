param name string

resource vault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: name
  location: resourceGroup().location
  properties: {
    enabledForDeployment: true
    enabledForTemplateDeployment: false
    enabledForDiskEncryption: false
    tenantId: subscription().tenantId
    accessPolicies: []
    sku: {
      name: 'standard'
      family: 'A'
    }
  }
}

// resource diagnostics 'microsoft.insights/diagnosticSettings@2017-05-01-preview' = if (!empty(logAnalyticsWrokspaceId)) {
//   name: 'diagnostics'
//   scope: vault
//   properties: {
//     workspaceId: logAnalyticsWrokspaceId
//     logs: [
//       {
//         category: 'AuditEvent'
//         enabled: true
//       }
//       // {
//       //   category: 'AllMetrics'
//       //   enabled: true
//       // }
//     ]
//   }
// }

output id string = vault.id
output name string = vault.name
