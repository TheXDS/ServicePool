# ServicePool

[![CodeFactor](https://www.codefactor.io/repository/github/thexds/servicepool/badge)](https://www.codefactor.io/repository/github/thexds/servicepool)
[![Build MCART](https://github.com/TheXDS/ServicePool/actions/workflows/build.yml/badge.svg)](https://github.com/TheXDS/ServicePool/actions/workflows/build.yml)
[![Publish MCART](https://github.com/TheXDS/ServicePool/actions/workflows/publish.yml/badge.svg)](https://github.com/TheXDS/ServicePool/actions/workflows/publish.yml)
[![Issues](https://img.shields.io/github/issues/TheXDS/ServicePool)](https://github.com/TheXDS/ServicePool/issues)
[![codecov](https://codecov.io/gh/TheXDS/ServicePool/branch/master/graph/badge.svg?token=Rve4awcyup)](https://codecov.io/gh/TheXDS/ServicePool)
[![GPL-v3.0](https://img.shields.io/github/license/TheXDS/ServicePool)](https://www.gnu.org/licenses/gpl-3.0.en.html)

## Releases
Release | Link
--- | ---
Latest public release: | [![Latest stable NuGet package](https://buildstats.info/nuget/TheXDS.ServicePool)](https://www.nuget.org/packages/TheXDS.ServicePool/)  
Latest development release: | [![Latest development NuGet package](https://buildstats.info/nuget/TheXDS.ServicePool?includePreReleases=true)](https://www.nuget.org/packages/TheXDS.ServicePool/)

**Package Manager**  
```sh
Install-Package TheXDS.ServicePool
```

**.NET CLI**  
```sh
dotnet add package TheXDS.ServicePool
```

**Paket CLI**  
```sh
paket add TheXDS.ServicePool
```

**Package reference**  
```xml
<PackageReference Include="TheXDS.ServicePool" Version="1.0.0" />
```

**C# interactive window (CSI)**  
```
#r "nuget: TheXDS.ServicePool, 1.0.0"
```

## Building
### Prerequisites
- [.Net SDK 5.0](https://dotnet.microsoft.com/) or higher.

### Build ServicePool
```sh
dotnet build ./src/ServicePool.sln
```
The resulting binaries will be in the `Build/bin` directory.

### Testing ServicePool
```sh
dotnet test ./src/ServicePool.sln
```
