# DeviceClientFactory

This library extends the basic IoT connection string to support additional scenarios such as DPS and certificates.

This library is available as a NuGet package.

[![Nuget](https://img.shields.io/nuget/vpre/Rido.DeviceClientFactory?style=flat-square)](https://www.nuget.org/packages/Rido.DeviceClientFactory)

## Sample Code

```cs
var deviceFactory = new DeviceClientFactory(_connectionString, _logger);
var deviceClient = await deviceFactory.CreateDeviceClient().ConfigureAwait(false);
```

## Sample connection strings

*Direct connection string* - connect to Hub only
```
HostName=myhub.azure-devices.net;DeviceId=myDevice;SharedAccessKey=asd8f789fa9s8u9suf9s8udf9as8uf8d
```

*ScopeId + Sas Key* - connect to Hub+DPS or Central, Device Key must be generated with `dps-keygen` using the masterkey
```
ScopeId=0ne123123;DeviceId=myDevice;SharedAccessKey=s0f98as0d9f8as0d89fsa0d89f0asd89fsadf
```

*ScopeId + Certificate* - connect to Hub+DPS or Central. Root cert must be verified in Central or DPS
```
ScopeId=0ne12312;X509Thumbprint=1231231423459243859328
```
The certificate must be avaiable, withing the private key, in the `CurrentUser\My` store.

*ScopeId + DCM + Sas or X509* connect to Hub+DPS or Central using a DCM Id
```
ScopeId=0ne12312;X509Thumbprint=1231231423459243859328;DcmId=urn:company:interface:1
```