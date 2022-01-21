param name string

resource config 'Microsoft.AppConfiguration/configurationStores@2021-03-01-preview' = {
  name: name
  location: resourceGroup().location
  sku: {
    name: 'Standard'
  }
}

output name string = name
