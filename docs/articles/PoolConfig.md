# Pool configuration in *ServicePool*

In this page we'll explore some basics on the pool configurations available by default in *ServicePool*

## What is pool configuration?
In *ServicePool* you may define a pool configuration as the behavior of the pool when trying to crate a new type, as well as how it resolves its dependencies.

These customizable configuration values are:
- **`DependencyResolver`**: When resolving a dependency for a specific type, you can specify how to obtain it by overriding this property. Whatever is returned by the specified method will be passed onto the constructor for the type that the pool is trying to resolve.
- **`SelfRegister`**: If set to `true`, this property instructs the pool to register itself as a resolvable dependency.
- **`TypeRegistrations`**: When registering a type or singleton on a pool, this value allows you to enumerate the types for which to register it. This way, you may resolve the same object using whatever base types or interfaces you require.
- **`ActiveResolver`**: Specifies the actual resolution logic for a type required by the service being requested. This property fetches dependencies from a collection of *active* dependencies, that is, from existing singletons in the pool.
- **`FactoryResolver`**: Like the `ActiveResolver` property, but using a collection of available factories to create a new instance of the required dependency.
- **`DiscoveryEngine`**: Whenever you request an object using the *discovery* consumption model, this property let's you specify how to enumerate available types in your app domain.
- **`ConstructorEnumeration`**: It let's you specify how to enumerate the constructors to try to use when creating a new object instance. Whenever a constructor seems unsuitable to be used for instancing, the pool will try the next one on this enumeration.

By defult, *ServicePool* ships with three customizable pool configurations.

### `PoolConfig.Default` pool configuration
This is the default configuration used by *ServicePool* if you don't specify a custom config to use for your pool.

It will:
- Use simple and recursive type resolution. This means that it will call into the pool to resolve any dependency for the service you request.
- Enable self-registration. The pool itself will be resolveable as a dependency.
- Register the type/singleton explicitly by type. That is, you may only resolve it by specifying the exact same type used to register it. This mirrors other dependency libraries, like Microsoft's own dependency injection library.
- Use a simple enumeration and match of type/factory for both active and factory registries.
- Use the `DefaultDiscoveryEngine`, which enumerates public types in the current `AppDomain`.
- Enumerate all public constructors for a service being resolved, sorted in descending order by the number of parameters they accept. This allows the pool to try instancing the requested service passing the most injectable dependencies accepted by it. If a constructor contains a dependency that is not available for resolution, the pool will try to use the next constructor until all options are exaxusted.

### `PoolConfig.FlexResolve`
This is a special pool configuration that inherits all configuration values from `PoolConfig.Default`, but replaces the `ActiveResolver` and `FactoryResolver` configurations to allow for resolving a type given any of its base types or interfaces.

### `PoolConfig.FlexRegister`
In contrast to `FlexResolve`, `FlexRegister` replaces the `TypeRegistrations` configuration from `Default` instead. It will enumerate all base types and interfaces and registers the dependency to be resolveable by ay of them.

### `FlexResolve` vs `FlexRegister`
Both of these configurations will allow you to resolve a service by any of their base types. Conside the following example:
```csharp
Pool pool = new Pool(/* use either FlexResolve or FlexRegister */)
List<int> myList = [];
pool.Register(myList);

// This resolves the service by its exact type.
_ = pool.Resolve<List<int>>();

// This also resolves the same service
_ = pool.Resolve<ICollection<int>>();
```
On the surface, it might seem that these two pool configurations behave pretty simmilarly, and for the most part, to the end user they do. But, there's some semantic differences that must be taken into account when choosing one or the other:

`FlexRegister` will allow you to **register** a single object to be resolvable by any of its base types or interfaces. This will have the side effect to disallow other services to be registered with any of the same base types. If you need to register two or more services that may share a single base type or interface, you may not want to use this configuration.

`FlexResolve` on the other hand, uses a different way of **resolving** a service by the base types, which allows several services to be registered that may have the same type, base types or interfaces. *ServicePool* will limit itself to use whatever service was registered first for a given service type.

### Further customization
If none of the builtin pool configurations suit your needs exactly, you can replace any of the configuration values when you create your pool, as the `PoolConfig` type is a C# 9 record.

You may create your custom pool configuration by doing the following:
```csharp
// This creates a custom pool config, based on FlexResolve.
PoolConfig myCustomConfig = PoolConfig.FlexResolve with {
    SelfRegister = false
};
```

## Using pool configurations in a pool
Whenever you create a new `Pool`, you can specify the configuration to be used as a constructor argument:
```csharp
// Create a pool that uses the FlexRegister pool configuration
Pool myPool = new Pool(PoolConfig.FlexRegister)

/* ...or roll your own:
Pool myPool = new Pool(myCustomConfig)
*/
```