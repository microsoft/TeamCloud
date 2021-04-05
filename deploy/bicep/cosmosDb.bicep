param name string

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

output connectionString string = listConnectionStrings(cosmos.id, cosmos.apiVersion).connectionStrings[0].connectionString

// output connectionString string = 'AccountEndpoint=${reference('Microsoft.DocumentDb/databaseAccounts/${name}').documentEndpoint};AccountKey=${listKeys(cosmos.id, '2015-04-08').primaryMasterKey}'
