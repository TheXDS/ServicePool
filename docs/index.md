# ServicePool

Welcome to the **`ServicePool`** documentation! in this page, you'll find information about the usage of **`ServicePool`** for dependency injection patterns and its public API.

## About ServicePool
**`ServicePool`** is a customizable dependency injection library that offers the well-known benefits of DI as seen on ASP.Net projects, but extends on this concept by offering different kinds of service comsumption models (singletons, factories, one-offs) and the ability to have more that one pool with a set of independently resolved services.

The dependency injection model implemented by this library also adds a bit more flexibility when resolving dependencies for a class, even allowing a consumer app to specify its own resolution logic.

## Features of ServicePool
**`ServicePool`** allows you to define your own pool of services, where any dependencies needed by them will be resolved from the local collection of registered services.
```cs
var myPool = new Pool();
myPool.Register<Random>();
```

You can also ask **`ServicePool`** to automatically discover any type you need without being registered in the pool first, and as long as there is an assembly loaded in the app domain that contains an implementation of the type you request. So, as long as it has dependencies that can be met, it will return an instance of it.
```cs
var myService = myPool.Discover<ILogger>();
```

A pool may be created using different configuration modes, using pre-defined config values from the `PoolConfig` record struct, or you can create your own, effectively customizing how your pool works.
```cs
var myPool = new Pool(PoolConfig.FlexResolve);
```

Going a bit further, you have the ability to fine-tune the dependency resolution used by **`ServicePool`** to discover dependencies for a service without having to register any of these. This is done overriding the available property on the configuration being passed onto your pool.
```cs
var myPool = new Pool(PoolConfig.FlexResolve with
{ 
    DependencyResolver = (pool, type) => pool.Discover(type)
});
var myService = myPool.Discover<ILogger>();
```
Several properties are available to fine-tune how your service pool works.

There's also more than one consumption model. You can register services that live forever in the pool, remaining active and available for resolution. This is useful for classes that need to preserve state or that must be active in the background (like a telemetry service) or instances that can be shared across several classes (like a single Log service available for everyone). You can also register services that once consumed, will be removed permanently from the pool. A use case for this can be for security and encryption, where there must not be a remmanant of either the encryption class nor its configuration. Also, you can choose to use the well-known classic consuption model available in other *dependency injection* libraries, where once requested, a new instance of the service will be created.
```cs
myPool.Register<Random>();
myPool.RegisterNow<MyTelemetryService>();
var mySettings = myPool.Consume<IEncryptionSettings>();
```

For services that you decide to register in a *persistent* way, you can ask **`ServicePool`** to instantiate them all if you haven't done so.
```cs
myPool.InitNow();
```

**`ServicePool`** Also provides the ability to filter out or focus the scope of type discovery search by assembly. You can choose to specify an Assembly enumerator that you control. You might choose to implement a Plugin system for your application using this feature by loading assemblies from wherever you need.
```cs
var myService = myPool.Discover<ILogger>(new MyOwnDiscoveryEngine());
```

In contrast to Microsoft's own dependency injection library, **`ServicePool`** Allows you to resolve services offering several interfaces without having to manually register them for each one.
```cs
myPool.Register<List<int>>();

var iEnumerable = myPool.Resolve<IEnumerable<int>>();
var iCollection = myPool.Resolve<ICollection<int>>();
var iList = myPool.Resolve<IList>();
```

Finally, **`ServicePool`** was designed with simplicity in mind, so the library itself is fairly small in size.