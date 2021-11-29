using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace TheXDS.ServicePool
{
    /// <summary>
    /// Represents a collection of hosted services that can be instanced and 
    /// resolved using dependency injection.
    /// </summary>
    public class ServicePool : IEnumerable
    {
        private record FactoryEntry(Type Key, bool Persistent, Func<object> Factory);
        private class TypeComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? x, object? y)
            {
                return x?.GetType().Equals(y?.GetType()) ?? y is null;
            }

            public int GetHashCode([DisallowNull] object obj)
            {
                return obj.GetType().GetHashCode();
            }
        }

        private readonly ICollection<FactoryEntry> _factories = new HashSet<FactoryEntry>();
        private readonly ICollection<object> _singletons = new HashSet<object>(new TypeComparer());

        /// <summary>
        /// Gets the number of actively instanced singletons registered in the
        /// pool.
        /// </summary>
        public int ActiveCount => _singletons.Count;

        /// <summary>
        /// Gets a count of all the registered services in this pool.
        /// </summary>
        public int Count => ActiveCount + _factories.Count;

        /// <summary>
        /// Registers a lazily-instanced singleton.
        /// </summary>
        /// <typeparam name="T">Type of service to register.</typeparam>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, the resolved singleton is going
        /// to be persisted in the service pool. When <see langword="false"/>,
        /// the registered service will be instanced and initialized each time
        /// it is requested.
        /// </param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool Register<T>(bool persistent = true) where T : notnull
        {
            return Register(CreateNewInstance<T>, persistent);
        }

        /// <summary>
        /// Registers a lazily-instanced singleton.
        /// </summary>
        /// <typeparam name="T">Type of service to register.</typeparam>
        /// <param name="factory">Singleton factory to use.</param>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, the resolved singleton is going
        /// to be persisted in the service pool. When <see langword="false"/>,
        /// the registered service will be instanced and initialized each time
        /// it is requested.
        /// </param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool Register<T>(Func<T> factory, bool persistent = true) where T : notnull
        {
            _factories.Add(new(typeof(T), persistent, () => factory()));
            return this;
        }

        /// <summary>
        /// Initializes all registered services marked as persistent using
        /// their respective registered factories.
        /// </summary>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool InitNow()
        {
            FactoryEntry[] entries = _factories.Where(p => p.Persistent).ToArray();
            foreach (FactoryEntry entry in entries)
            {
                RegisterNow(entry.Factory());
                _factories.Remove(entry);
            }
            return this;
        }

        /// <summary>
        /// Instances and registers a new service of type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to register.</typeparam>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool RegisterNow<T>() where T : notnull
        {
            return RegisterNow(CreateNewInstance<T>());
        }

        /// <summary>
        /// Registers a new instance of the specified service.
        /// </summary>
        /// <param name="singleton">Instance of the service.</param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool RegisterNow(object singleton)
        {
            _singletons.Add(singleton);
            return this;
        }

        /// <summary>
        /// Registers a lazily-instanced singleton of type
        /// <typeparamref name="T"/> if the condition is true.
        /// </summary>
        /// <typeparam name="T">Type of service to register.</typeparam>
        /// <param name="condition">
        /// Determines if the service will be added to this pool.
        /// </param>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, the resolved singleton is going
        /// to be persisted in the service pool. When <see langword="false"/>,
        /// the registered service will be instanced and initialized each time
        /// it is requested.
        /// </param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool RegisterIf<T>(bool condition, bool persistent = true) where T : notnull
        {
            if (condition) Register<T>(persistent);
            return this;
        }

        /// <summary>
        /// Registers a lazily-instanced singleton of type
        /// <typeparamref name="T"/> if the condition is true.
        /// </summary>
        /// <param name="condition">
        /// Determines if the service will be added to this pool.
        /// </param>
        /// <param name="factory">Singleton factory to use.</param>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, the resolved singleton is going
        /// to be persisted in the service pool. When <see langword="false"/>,
        /// the registered service will be instanced and initialized each time
        /// it is requested.
        /// </param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool RegisterIf<T>(bool condition, Func<T> factory, bool persistent = true) where T : notnull
        {
            if (condition) Register(factory, persistent);
            return this;
        }

        /// <summary>
        /// Instances and registers a new service of type
        /// <typeparamref name="T"/> if the condition is true.
        /// </summary>
        /// <typeparam name="T">Type of service to register.</typeparam>
        /// <param name="condition">
        /// Determines if the service will be added to this pool.
        /// </param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool RegisterNowIf<T>(bool condition) where T : notnull
        {
            if (condition) RegisterNow<T>();
            return this;
        }

        /// <summary>
        /// Instances and registers a new service if the condition is true.
        /// </summary>
        /// <param name="condition">
        /// Determines if the service will be added to this pool.
        /// </param>
        /// <param name="singleton">Instance of the service.</param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool RegisterNowIf(bool condition, object singleton)
        {
            if (condition) RegisterNow(singleton);
            return this;
        }

        /// <summary>
        /// Tries to resolve a service that implements the specified interface 
        /// or base type.
        /// </summary>
        /// <typeparam name="T">Type of service to get.</typeparam>
        /// <returns>
        /// The first service that implements <typeparamref name="T"/> found on
        /// the pool, or <see langword="null"/> if no such service has been
        /// registered.
        /// </returns>
        public T? Resolve<T>() where T : notnull => (T?)Resolve(typeof(T));

        /// <summary>
        /// Tries to resolve a registered service of type
        /// <typeparamref name="T"/>, and if not found, searches for any type
        /// in the app domain that can be instanced and returned as the
        /// requested service.
        /// </summary>
        /// <typeparam name="T">Type of service to get.</typeparam>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, in case a service of the
        /// specified type hasn't been registered and a compatible type has
        /// been discovered, the newly created instance will be registered
        /// persistently in the pool. If set to <see langword="false"/>, the
        /// discovered service will not be added to the pool.
        /// </param>
        /// <returns>
        /// A registered service or a newly discovered one if it implements the
        /// requested type, or <see langword="null"/> in case that no
        /// discoverable service for the requested type exists.
        /// </returns>
        public T? Discover<T>(bool persistent = true) where T : notnull
        {
            return (T?)Discover(typeof(T), persistent).FirstOrDefault();
        }

        /// <summary>
        /// Removes a singleton from this service pool.
        /// </summary>
        /// <param name="service">Service instance to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the service instance was found and
        /// removed from the pool, <see langword="false"/> otherwise.
        /// </returns>
        public bool Remove(object service)
        {
            return _singletons.Remove(service);
        }

        /// <summary>
        /// Removes a registered service from this service pool.
        /// </summary>
        /// <typeparam name="T">Type of service to remove.</typeparam>
        /// <returns>
        /// <see langword="true"/> if a registered service matching the
        /// requested type was found and removed from the pool,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Remove<T>()
        {
            return ResolveActive(typeof(T)) is { } o ? Remove(o) : (GetLazyFactory(typeof(T)) is { } f && _factories.Remove(f));
        }

        /// <summary>
        /// Resolves and then removes a registered service from this service
        /// pool.
        /// </summary>
        /// <typeparam name="T">Type of service to consume.</typeparam>
        /// <returns>
        /// The first service that implements <typeparamref name="T"/> found on
        /// the pool, or <see langword="null"/> if no such service has been
        /// registered.
        /// </returns>
        public T? Consume<T>() where T : notnull
        {
            T? obj = Resolve<T>();
            if (obj is not null) Remove(obj);
            return obj;
        }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_singletons).GetEnumerator();
        }

        private IEnumerable<object?> Discover(Type t, bool persistent)
        {
            if (Resolve(t) is { } o) yield return o;
            if (AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(p => p.GetExportedTypes())
                .FirstOrDefault(p => !p.IsAbstract && !p.IsInterface && t.IsAssignableFrom(p)) is Type dt
                && CreateNewInstance(dt) is { } obj)
            {
                if (persistent) RegisterNow(obj);
                yield return obj;
            }
        }

        private object? CreateNewInstance(Type t)
        {
            if (t.IsAbstract || t.IsInterface) return null;
            ConstructorInfo[] ctors = t.GetConstructors().OrderByDescending(p => p.GetParameters().Length).ToArray();
            foreach (var ctor in ctors)
            {
                ParameterInfo[] pars = ctor.GetParameters();
                List<object> args = new();
                foreach (ParameterInfo arg in pars)
                {
                    var value = Resolve(arg.ParameterType) ?? (arg.IsOptional ? Type.Missing : null);
                    if (value is null) break;
                    args.Add(value);
                }
                if (args.Count == pars.Length)
                {
                    return ctor.Invoke(args.ToArray());                    
                }
            }
            return null;
        }

        private T CreateNewInstance<T>()
        {
            return (T)(CreateNewInstance(typeof(T)) ?? throw new InvalidOperationException(Resources.Strings.Errors.CantInstance));
        }

        private object? Resolve(Type serviceType)
        {
            return ResolveActive(serviceType) ?? ResolveLazy(serviceType);
        }

        private object? ResolveActive(Type serviceType)
        {
            return _singletons.FirstOrDefault(p => p.GetType() == serviceType);
        }

        private object? ResolveLazy(Type serviceType)
        {
            if (GetLazyFactory(serviceType) is { Factory: { } factMethod, Persistent: { } persistent } factory)
            {
                var obj = factMethod.Invoke();
                if (persistent)
                {
                    _factories.Remove(factory);
                    RegisterNow(obj);
                }
                return obj;
            }
            return null;
        }

        private FactoryEntry? GetLazyFactory(Type serviceType)
        {
            return _factories.FirstOrDefault(p => serviceType.IsAssignableFrom(p.Key));
        }
    }
}
