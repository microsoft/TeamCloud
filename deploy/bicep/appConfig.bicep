param name string

resource config 'Microsoft.AppConfiguration/configurationStores@2020-07-01-preview' = {
  name: name
  location: resourceGroup().location
  sku: {
    name: 'Standard'
  }
}

output id string = config.id
output connectionString string = listKeys(config.id, '2019-10-01').value[0].connectionString
