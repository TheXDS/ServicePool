# ServicePool

[![CodeFactor](https://www.codefactor.io/repository/github/thexds/servicepool/badge)](https://www.codefactor.io/repository/github/thexds/servicepool)
[![Build MCART](https://github.com/TheXDS/ServicePool/actions/workflows/build.yml/badge.svg)](https://github.com/TheXDS/ServicePool/actions/workflows/build.yml)
[![Publish MCART](https://github.com/TheXDS/ServicePool/actions/workflows/publish.yml/badge.svg)](https://github.com/TheXDS/ServicePool/actions/workflows/publish.yml)
[![Issues](https://img.shields.io/github/issues/TheXDS/ServicePool)](https://github.com/TheXDS/ServicePool/issues)
[![codecov](https://codecov.io/gh/TheXDS/ServicePool/branch/master/graph/badge.svg?token=Rve4awcyup)](https://codecov.io/gh/TheXDS/ServicePool)
[![GPL-v3.0](https://img.shields.io/github/license/TheXDS/ServicePool)](https://www.gnu.org/licenses/gpl-3.0.en.html)

ServicePool is a customizable dependency injection library that offers the well-known benefits of DI as seen on ASP.Net projects, but extends on this concept by offering different kinds of service comsumption models (singletons, factories, one-offs) and the ability to have more that one pool with a set of independently resolved services.

The dependency injection model implemented by this library also adds a bit more flexibility when resolving dependencies for a class, even allowing a consumer app to specify its own resolution logic.

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
<PackageReference Include="TheXDS.ServicePool" Version="1.0.4" />
```

**C# interactive window (CSI)**  
```
#r "nuget: TheXDS.ServicePool, 1.0.4"
```

## Building
ServicePool can be built on any platform or CI environment supported by dotnet.

### Prerequisites
- [.Net SDK 6.0](https://dotnet.microsoft.com/)

### Build ServicePool
```sh
dotnet build ./src/ServicePool.sln
```
The resulting binaries will be in the `./Build/bin` directory.

### Testing ServicePool
```sh
dotnet test ./src/ServicePool.sln
```
#### Coverage reports
It is possible to generate a coverage report locally.For that, it is necessary to install [`ReportGenerator`](https://github.com/danielpalme/ReportGenerator), which will read the test results after execution, and will generate a web page with the coverage results.

To install `ReportGenerator` execute:
```sh
dotnet tool install -g dotnet-reportgenerator-globaltool
```
After installing `ReportGenerator`, it will be possible to execute the following command:
```sh
dotnet test ./src/ServicePool.sln --collect:"XPlat Code Coverage" --results-directory:./Build/Tests ; reportgenerator -reports:./Build/Tests/*/coverage.cobertura.xml -targetdir:./Build/Coverage/
```
The coverage reports will be stored in `./Build/Coverage`

## Contribute
[![Buy Me A Coffee](https://cdn.buymeacoffee.com/buttons/default-orange.png)](https://www.buymeacoffee.com/xdsxpsivx)

If `ServicePool` is useful to you, or if you're interested in donating to sponsor the project, feel free to to a donation via [PayPal](https://paypal.me/thexds), [BuyMeACoffee](https://www.buymeacoffee.com/xdsxpsivx) or just contact me directly.

Sadly, I cannot offer other means of sending donations as of right now due to my country (Honduras) not being supported by almost any platform.
