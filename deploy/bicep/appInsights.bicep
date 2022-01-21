param name string

resource ai 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output name string = name
