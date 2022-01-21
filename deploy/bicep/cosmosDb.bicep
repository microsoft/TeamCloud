param name string
param appConfigName string

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2021-01-15' = {
  name: name
  location: resourceGroup().location
  kind: 'GlobalDocumentDB'
  tags: {
    defaultExperience: 'DocumentDB'
  }
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: resourceGroup().location
        failoverPriority: 0
      }
    ]
  }
}

module cosmosConfigs 'appConfigKeys.bicep' = {
  name: 'cosmosConfigs'
  params: {
    configName: appConfigName
    keyValues: {
      'Azure:CosmosDb:TenantName': 'TeamCloud'
      'Azure:CosmosDb:DatabaseName': 'TeamCloud'
      'Azure:CosmosDb:ConnectionString': cosmos.listConnectionStrings().connectionStrings[0].connectionString
    }
  }
}
