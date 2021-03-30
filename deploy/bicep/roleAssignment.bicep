param name string
param principalId string
param roleDefinitionId string

resource role 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: name
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
