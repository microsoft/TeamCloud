param location string = resourceGroup().location
param name string
param serviceUrl string
param repository string
param registryName string = 'TeamCloud'

resource api_webhook 'Microsoft.ContainerRegistry/registries/webhooks@2021-09-01' = {
  name: '${registryName}/${name}'
  location: location
  properties: {
    actions: [
      'push'
    ]
    scope: repository
    serviceUri: serviceUrl
  }
}
