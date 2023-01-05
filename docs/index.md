# ServicePool

Welcome to the ServicePool documentation! in this page, you'll find information about the usage of ServicePool for dependency injection patterns and its public API.

## About ServicePool
ServicePool is a customizable dependency injection library that offers the well-known benefits of DI as seen on ASP.Net projects, but extends on this concept by offering different kinds of service comsumption models (singletons, factories, one-offs) and the ability to have more that one pool with a set of independently resolved services.

The dependency injection model implemented by this library also adds a bit more flexibility when resolving dependencies for a class, even allowing a consumer app to specify its own resolution logic.