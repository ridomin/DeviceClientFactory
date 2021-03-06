# DeviceClientFactory

[![Gitpod Ready-to-Code](https://img.shields.io/badge/Gitpod-Ready--to--Code-blue?logo=gitpod)](https://gitpod.io/#https://github.com/ridomin/DeviceClientFactory)

This library extends the basic IoT connection string to support additional scenarios such as DPS and certificates.

This library is available as a NuGet package.

[![Nuget](https://img.shields.io/nuget/vpre/Rido.DeviceClientFactory?style=flat-square)](https://www.nuget.org/packages/Rido.DeviceClientFactory)

![.NET Core](https://github.com/ridomin/DeviceClientFactory/workflows/.NET%20Core/badge.svg)

## Sample Code

```cs
var deviceClient = await DeviceClientFactory.CreateDeviceClientAsync(connectionString, modelId);
```

## Sample connection strings

*Direct connection string* - connect to Hub only

```cs
string connectionString = "HostName=myhub.azure-devices.net;DeviceId=myDevice;SharedAccessKey=asd8f789fa9s8u9suf9s8udf9as8uf8d";
DeviceClient dc = await DeviceClientFactory.CreateDeviceClientAsync(connectionString);
```

*Direct connection string with Model Id* - PnP Discovery

```cs
string connectionString = "HostName=myhub.azure-devices.net;DeviceId=myDevice;SharedAccessKey=asd8f789fa9s8u9suf9s8udf9as8uf8d";
string modelId = "dtmi:com:example:TemperatureController;1";
DeviceClient dc = await DeviceClientFactory.CreateDeviceClientAsync(connectionString, modelId);
```


*ScopeId + Sas Key* - connect to Hub+DPS or Central, Device Key must be generated with `dps-keygen` using the masterkey

```cs
string connectionString = "ScopeId=0ne123123;DeviceId=myDevice;SharedAccessKey=s0f98as0d9f8as0d89fsa0d89f0asd89fsadf";
string modelId = "dtmi:com:example:TemperatureController;1";
DeviceClient dc = await DeviceClientFactory.CreateDeviceClientAsync(connectionString, modelId);
```

*ScopeId + Certificate* - connect to Hub+DPS or Central. Root cert must be verified in Central or DPS

```cs
string connectionString = "ScopeId=0ne12312;X509=1231231423459243859328";
DeviceClient dc = await DeviceClientFactory.CreateDeviceClientAsync(connectionString);
```

The certificate must be avaiable, within the private key, in the `CurrentUser\My` cert store or as a pfx file

*ScopeId + ModelId + Sas or X509* connect to Hub+DPS or Central using a Model Id

```cs
string connectionString = "ScopeId=0ne12312;X509T=1231231423459243859328";
string modelId = "dtmi:com:example:TemperatureController;1";
DeviceClient dc = await DeviceClientFactory.CreateDeviceClientAsync(connectionString, modelId);
```


### Note about certificates

Certificates can be loaded from the CurrentUser\My store by thumbprint

```text
X509=123123123123123
```

or by filename. The pfx file should be copied to the output location

```text
X509=mycert.pfx
```

and optionally can include a password

```text
X509=mycert.pfx|password
```
