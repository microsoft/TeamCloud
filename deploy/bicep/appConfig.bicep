param location string = resourceGroup().location
param name string

resource config 'Microsoft.AppConfiguration/configurationStores@2021-03-01-preview' = {
  name: name
  location: location
  sku: {
    name: 'Standard'
  }
}

output name string = name
