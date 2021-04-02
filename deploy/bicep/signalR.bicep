param name string

resource signalr 'Microsoft.SignalRService/signalR@2020-07-01-preview' = {
  name: name
  location: resourceGroup().location
  sku: {
    name: 'Standard_S1'
    tier: 'Standard'
    capacity: 1
  }
  kind: 'SignalR'
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        'flag': 'ServiceMode'
        'value': 'Serverless'
        'properties': {}
      }
      {
        'flag': 'EnableConnectivityLogs'
        'value': 'True'
        'properties': {}
      }
      {
        'flag': 'EnableMessagingLogs'
        'value': 'False'
        'properties': {}
      }
    ]
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
    upstream: {}
    networkACLs: {
      defaultAction: 'Deny'
      publicNetwork: {
        allow: [
          'ServerConnection'
          'ClientConnection'
          'RESTAPI'
        ]
      }
      privateEndpoints: []
    }
  }
}

output connectionString string = listKeys(signalr.id, '2020-05-01').primaryConnectionString
