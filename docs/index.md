# ServicePool

Welcome to the **`ServicePool`** documentation! in this page, you'll find information about the usage of **`ServicePool`** for dependency injection patterns and its public API.

## About ServicePool
**`ServicePool`** is a customizable dependency injection library that offers the well-known benefits of DI as seen on ASP.Net projects, but extends on this concept by offering different kinds of service comsumption models (singletons, factories, one-offs) and the ability to have more that one pool with a set of independently resolved services.

The dependency injection model implemented by this library also adds a bit more flexibility when resolving dependencies for a class, even allowing a consumer app to specify its own resolution logic.

## Features of ServicePool
By design, **`ServicePool`** can be used in one of several ways. You can just use the `ServicePool` class statically, or create instances of it to granularly control your services and their dependencies.
```cs
ServicePool.CommonPool.Register<Random>();
// ...or
var myPool = new ServicePool();
myPool.Register<Random>();
```

You can also ask **`ServicePool`** to automatically discover any type you need without being registered in the pool first, and as long as there is an assembly loaded in the app domain that contains an implementation of the type you request. So, as long as it has dependencies that can be met, it will return an instance of it.
```cs
var myService = myPool.Discover<ILogger>();
```

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