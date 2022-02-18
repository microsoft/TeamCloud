param location string = resourceGroup().location
param functionAppName string
param utcValue string = utcNow()

var identityName = 'sleepHackIdentity${utcValue}'

var roleAssignmentIdName = guid('${resourceGroup().id}${identityName}contributor${utcValue}')
var contributorRoleDefinitionId = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/b24988ac-6180-42a0-ab88-20f7382dd24c'

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: identityName
  location: location
}

resource roleAssignmentId 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: roleAssignmentIdName
  properties: {
    roleDefinitionId: contributorRoleDefinitionId
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource script 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  kind: 'AzureCLI'
  name: 'sleepHack'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    // forceUpdateTag: utcValue
    azCliVersion: '2.30.0'
    timeout: 'PT1H'
    cleanupPreference: 'Always'
    retentionInterval: 'PT1H'
    scriptContent: 'counter=0; until az functionapp keys list -g ${resourceGroup().name} -n ${functionAppName} --query functionKeys.default > /dev/null; do echo "Function host key not available yet, retrying in 10 seconds..."; if [ $counter -ge 30 ]; then break; fi; ((counter++)); sleep 10; done; echo "done"; az identity delete --ids "$AZ_SCRIPTS_USER_ASSIGNED_IDENTITY"'
  }
  dependsOn: [
    roleAssignmentId
  ]
}
