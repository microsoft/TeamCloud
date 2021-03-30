param name string

resource storage 'Microsoft.Storage/storageAccounts@2020-08-01-preview' = {
  name: name
  location: resourceGroup().location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_RAGRS'
    tier: 'Standard'
  }
  properties: {}
}

output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${name};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
