param configName string
@secure()
param keyValues object

resource keyValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2021-03-01-preview' = [for kv in items(keyValues): {
  name: '${configName}/${kv.key}'
  properties: {
    value: kv.value
  }
}]
