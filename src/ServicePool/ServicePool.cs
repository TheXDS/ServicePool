// ServicePool.cs
//
// This file is part of ServicePool
//
// Author(s):
//      César Andrés Morgan <xds_xps_ivx@hotmail.com>
//
// Copyright (C) 2021 César Andrés Morgan
//
// ServicePool is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later
// version.
//
// ServicePool is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY, without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheXDS.ServicePool.Resources;

namespace TheXDS.ServicePool
{
    /// <summary>
    /// Represents a collection of hosted services that can be instanced and 
    /// resolved using dependency injection.
    /// </summary>
    public class ServicePool : IEnumerable
    {
        private static ServicePool? _commonPool;

        /// <summary>
        /// Gets a static reference to a common service pool.
        /// </summary>
        /// <remarks>
        /// This property will instance the static pool upon it being accessed
        /// for the first time, and the pool will live throughout the lifespan
        /// of the application.
        /// </remarks>
        public static ServicePool CommonPool
        {
            get => _commonPool ??= new();
        }

        private record FactoryEntry(Type Key, bool Persistent, Func<object> Factory);

        private readonly ICollection<FactoryEntry> _factories = new HashSet<FactoryEntry>();
        private readonly ICollection<object> _singletons = new HashSet<object>();

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
            return condition ? Register<T>(persistent) : this;
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
            return condition ? Register<T>(factory, persistent) : this;
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
            return condition ? RegisterNow<T>() : this;
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
            return condition ? RegisterNow(singleton) : this;
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
        /// Resolves all services that match the requested type.
        /// </summary>
        /// <typeparam name="T">Type fo service to resolve.</typeparam>
        /// <returns>
        /// An enumeration of all the services (both eagerly and lazily
        /// initialized) registered in the pool that implement the specified
        /// type.
        /// </returns>
        public IEnumerable<T> ResolveAll<T>() where T : notnull
        {
            return _singletons.OfType<T>()
                .Concat(GetLazyFactory(typeof(T))
                .Select(CreateFromLazy).Cast<T>());
        }

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
        /// <remarks>
        /// When discovering new services, if a service of a specific type is
        /// found inside the pool, it will be gracefully skipped and not
        /// instanced again.
        /// </remarks>
        public T? Discover<T>(bool persistent = true) where T : notnull
        {
            return Discover<T>(new DefaultDiscoveryEngine(), persistent);
        }

        /// <summary>
        /// Tries to resolve a registered service of type
        /// <typeparamref name="T"/>, and if not found, searches for any type
        /// that can be instanced and returned as the requested service.
        /// </summary>
        /// <typeparam name="T">Type of service to get.</typeparam>
        /// <param name="discoveryEngine">
        /// Discovery engine to use while searching for new instantiable types.
        /// </param>
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
        /// <remarks>
        /// When discovering new services, if a service of a specific type is
        /// found inside the pool, it will be gracefully skipped and not
        /// instanced again.
        /// </remarks>
        public T? Discover<T>(IDiscoveryEngine discoveryEngine, bool persistent = true) where T : notnull
        {
            return Resolve(typeof(T)) is T o ? o : (T?)Discover(discoveryEngine, typeof(T), persistent).FirstOrDefault();
        }

        /// <summary>
        /// Tries to resolve and register all services of type
        /// <typeparamref name="T"/> found in the current app domain, returning
        /// the resulting enumeration of all services found.
        /// </summary>
        /// <typeparam name="T">Type of service to get.</typeparam>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, in case a service of the
        /// specified type hasn't been registered and a compatible type has
        /// been discovered, the newly created instance will be registered
        /// persistently in the pool. If set to <see langword="false"/>, any
        /// discovered service will not be added to the pool.
        /// </param>
        /// <returns>
        /// A collection of all the services found in the current app domain,
        /// or an empty enumeration in case that no discoverable service for
        /// the requested type exists.
        /// </returns>
        /// <remarks>
        /// The resulting enumeration will contain all registered services, and
        /// the discovery will skip any discoverable service for which there's
        /// a singleton with the same type or a compatible lazy factory
        /// registered.
        /// </remarks>
        public IEnumerable<T> DiscoverAll<T>(bool persistent = true) where T : notnull
        {
            return DiscoverAll<T>(new DefaultDiscoveryEngine(), persistent);
        }

        /// <summary>
        /// Tries to resolve and register all services of type
        /// <typeparamref name="T"/> found using the specified
        /// <see cref="IDiscoveryEngine"/>, returning the resulting enumeration
        /// of all services found.
        /// </summary>
        /// <typeparam name="T">Type of service to get.</typeparam>
        /// <param name="discoveryEngine">
        /// Discovery engine to use while searching for new instantiable types.
        /// </param>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, in case a service of the
        /// specified type hasn't been registered and a compatible type has
        /// been discovered, the newly created instance will be registered
        /// persistently in the pool. If set to <see langword="false"/>, any
        /// discovered service will not be added to the pool.
        /// </param>
        /// <returns>
        /// A collection of all the services found in the current app domain,
        /// or an empty enumeration in case that no discoverable service for
        /// the requested type exists.
        /// </returns>
        /// <remarks>
        /// The resulting enumeration will contain all registered services, and
        /// the discovery will skip any discoverable service for which there's
        /// a singleton with the same type or a compatible lazy factory
        /// registered.
        /// </remarks>
        public IEnumerable<T> DiscoverAll<T>(IDiscoveryEngine discoveryEngine, bool persistent = true) where T : notnull
        {
            return ResolveAll<T>().Concat(Discover(discoveryEngine, typeof(T), persistent).Cast<T>());
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
            return ResolveActive(typeof(T)) is { } o ? Remove(o) : (GetLazyFactory(typeof(T)).FirstOrDefault() is { } f && _factories.Remove(f));
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
            if (obj is not null) _ = Remove(obj) || Remove<T>();
            return obj;
        }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_singletons.Concat(_factories.Select(CreateFromLazy))).GetEnumerator();
        }

        private IEnumerable<object?> Discover(IDiscoveryEngine discoveryEngine, Type t, bool persistent)
        {
            foreach (Type dt in discoveryEngine.Discover(t))
            {
                if (Resolve(dt) is null && CreateNewInstance(dt) is { } obj)
                {
                    if (persistent) RegisterNow(obj);
                    yield return obj;
                }
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
                    var value =
                        (t.IsAssignableFrom(arg.ParameterType) ? ResolveActive(arg.ParameterType) : Resolve(arg.ParameterType)) ??
                        (arg.IsOptional ? Type.Missing : null);
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
            return (T)(CreateNewInstance(typeof(T)) ?? throw Errors.CantInstance());
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
            return GetLazyFactory(serviceType).FirstOrDefault() is { } factory
                ? CreateFromLazy(factory)
                : null;
        }

        private object CreateFromLazy(FactoryEntry factory)
        {
            var obj = factory.Factory.Invoke();
            if (factory.Persistent)
            {
                _factories.Remove(factory);
                RegisterNow(obj);
            }
            return obj;
        }

        private IEnumerable<FactoryEntry> GetLazyFactory(Type serviceType)
        {
            return _factories.Where(p => serviceType.IsAssignableFrom(p.Key));
        }
    }
}
