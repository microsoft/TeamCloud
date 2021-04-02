param name string

resource ai 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: name
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output id string = ai.id
output instrumentationKey string = ai.properties.InstrumentationKey
