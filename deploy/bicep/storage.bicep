param location string = resourceGroup().location
param name string
param appConfigName string = ''
param appConfigConnectionStringKeys array = []
param containers array = []

var addConnectionStringConfig = !empty(appConfigName) && !empty(appConfigConnectionStringKeys)

resource storage 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: name
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_RAGRS'
  }
  properties: {}

  resource blobServices 'blobServices' = if (!empty(containers)) {
    name: 'default'

    resource blobContainers 'containers' = [for (container, i) in containers: if (!empty(containers)) {
      name: container
    }]
  }
}

module storageConnectionStringConfig 'appConfigKeys.bicep' = [for (key, i) in appConfigConnectionStringKeys: if (addConnectionStringConfig) {
  name: '${name}Configs${i}'
  params: {
    configName: appConfigName
    keyValues: {
      '${key}': 'DefaultEndpointsProtocol=https;AccountName=${name};AccountKey=${storage.listKeys().keys[0].value}'
    }
  }
}]

output name string = name
