param name string

resource redis 'Microsoft.Cache/redis@2020-06-01' = {
  name: name
  location: resourceGroup().location
  properties: {
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    sku: {
      name: 'Standard'
      family: 'C'
      capacity: 4
    }
  }
}

output configuration string = '${name}.redis.cache.windows.net,abortConnect=false,ssl=true,password=${listKeys(redis.id, redis.apiVersion).primaryKey}'
