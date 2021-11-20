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

        public int ActiveCount => _singletons.Count;
        public int Count => ActiveCount + _factories.Count;
        

        /// <summary>
        /// registers a lazily-instanced singleton.
        /// </summary>
        /// <typeparam name="T">Type of service to register.</typeparam>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, the resolved singleton is going
        /// to be persisted in the service pool.
        /// </param>
        /// <returns>
        /// This same service pool instance, allowing the use of Fluent syntax.
        /// </returns>
        public ServicePool Register<T>(bool persistent = true) where T : notnull, new()
        {
            return Register(CreateNewInstance<T>, persistent);
        }

        /// <summary>
        /// registers a lazily-instanced singleton.
        /// </summary>
        /// <typeparam name="T">Type of service to register.</typeparam>
        /// <param name="factory">Singleton factory to use.</param>
        /// <param name="persistent">
        /// If set to <see langword="true"/>, the resolved singleton is going
        /// to be persisted in the service pool.
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
        /// Instances and registers a new service of type
        /// <typeparamref name="T"/> if the condition is true.
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

        public T? ResolveActive<T>() where T : notnull => (T?)ResolveActive(typeof(T));

        public T? ResolveLazy<T>() where T : notnull => (T?)ResolveLazy(typeof(T));

        public T? Discover<T>(bool persistent = true) where T : notnull
        {
            if (Resolve(typeof(T)) is T o) return o;

            foreach (var t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetExportedTypes()).Where(p => !p.IsAbstract && !p.IsInterface))
            {
                if (typeof(T).IsAssignableFrom(t))
                {
                    var obj =  CreateNewInstance<T>();
                    if (persistent) RegisterNow(obj);
                    return obj;
                }
            }
            return default;
        }





        /// <summary>
        /// Removes a singleton from this service pool.
        /// </summary>
        /// <param name="service"></param>
        public bool Remove(object service)
        {
            return _singletons.Remove(service);
        }








        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_singletons).GetEnumerator();
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
            if (_factories.FirstOrDefault(p => serviceType.IsAssignableFrom(p.Key)) is { Factory: { } factMethod, Persistent: { } persistent } factory)
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
    }

    /// <summary>
    /// Encapsulates a short-lived singleton, instanced only after being
    /// required from the Service Pool and destroyed when no longer needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DisposableSingleton<T> : IDisposable where T : notnull
    {
        private readonly ServicePool pool;

        internal DisposableSingleton(T instance, ServicePool pool)
        {
            Instance = instance;
            this.pool = pool;
        }

        /// <summary>
        /// Gets a reference to the instance requested from the service pool.
        /// </summary>
        public T Instance { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            (Instance as IDisposable)?.Dispose();
            pool.Remove(Instance);
        }

        /// <summary>
        /// Implicitly converts a <see cref="DisposableSingleton{T}"/> to a 
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <param name="disposableSingleton">Object to convert.</param>
        public static implicit operator T(DisposableSingleton<T> disposableSingleton)
        {
            return disposableSingleton.Instance;
        }
    }
}
