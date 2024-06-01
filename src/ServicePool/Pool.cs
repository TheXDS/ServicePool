// Pool.cs
//
// This file is part of ServicePool
//
// Author(s):
//      César Andrés Morgan <xds_xps_ivx@hotmail.com>
//
// Released under the MIT License (MIT)
// Copyright © 2011 - 2022 César Andrés Morgan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the “Software”), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheXDS.ServicePool.Resources;

namespace TheXDS.ServicePool;

/// <summary>
/// Represents a collection of hosted services that can be instantiated and 
/// resolved using dependency injection, resolving objects to whichever
/// resolution type they have registered, and only allowing a single type per
/// pool to be registered.
/// </summary>
public sealed class Pool(PoolConfig config) : IEnumerable
{
    private record FactoryEntry(in bool Persistent, in Func<object> Factory);

    private readonly PoolConfig _config = config;
    private readonly Dictionary<Type, FactoryEntry> _factories = [];
    private readonly Dictionary<Type, object> _instances = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Pool"/> class using the
    /// default pool configuration.
    /// </summary>
    public Pool() : this(PoolConfig.Default)
    {
    }

    /// <summary>
    /// Gets a count of all the registered services in this pool.
    /// </summary>
    public int Count => _instances.Count + _factories.Count;

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
    public T? Consume<T>() where T : notnull => (T?)Consume(typeof(T));

    /// <summary>
    /// Resolves and then removes a registered service from this service
    /// pool.
    /// </summary>
    /// <param name="objectType">Type of service to consume.</param>
    /// <returns>
    /// The first service that implements <paramref name="objectType"/> found
    /// on the pool, or <see langword="null"/> if no such service has been
    /// registered.
    /// </returns>
    public object? Consume(Type objectType)
    {
        object? obj = Resolve(objectType);
        if (obj is not null) _ = Remove(objectType);
        return obj;
    }

    /// <summary>
    /// Registers a lazily-instantiated service.
    /// </summary>
    /// <typeparam name="T">Type of object to register.</typeparam>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    public void Register<T>(bool persistent = true) where T : notnull => Register(typeof(T), persistent);

    /// <summary>
    /// _instances and registers a service of type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <param name="singleton">Singleton instance to register.</param>
    /// <typeparam name="T">Type of service to register.</typeparam>
    public void RegisterNow<T>(T singleton) where T : notnull => RegisterNow((object)singleton);

    /// <summary>
    /// _instances and registers a new service of type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    public void RegisterNow<T>() where T : notnull => RegisterNow(CreateInstance(typeof(T)));

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
    /// Tries to resolve a service that implements the specified interface 
    /// or base type.
    /// </summary>
    /// <param name="objectType">Type of service to get.</param>
    /// <returns>
    /// The first service that implements <paramref name="objectType"/> found
    /// on the pool, or <see langword="null"/> if no such service has been
    /// registered.
    /// </returns>
    public object? Resolve(Type objectType)
    {
        return ResolveActive(objectType) ?? ResolveLazy(objectType);
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
    public bool Remove<T>() => Remove(typeof(T));

    /// <summary>
    /// Creates a new instance of the specified type, resolving its required
    /// dependencies from the registered services on this instance.
    /// </summary>
    /// <param name="t">Type of object to instance.</param>
    /// <returns>
    /// A new instance of the specified type, or <see langword="null"/> if the
    /// specified type cannot be instantiated given its required dependencies.
    /// </returns>
    public object? CreateInstanceOrNull(Type t)
    {
        if (t.IsAbstract || t.IsInterface) return null;
        return CreateInstance_Internal(t);
    }

    /// <summary>
    /// Creates a new instance of the specified type, resolving its required
    /// dependencies from the registered services on this instance.
    /// </summary>
    /// <param name="t">Type of object to instance.</param>
    /// <returns>A new instance of the specified type.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the requested service could not be created given its required
    /// dependencies.
    /// </exception>
    public object CreateInstance(Type t)
    {
        ConstructorInfo[]? ctors = null;
        if (t.IsAbstract || t.IsInterface || (ctors = EnumerateCtors(t).ToArray()).Length == 0)
        {
            throw Errors.TypeNotInstantiable(t);
        }
        return CreateInstance_Internal(t) 
            ?? throw Errors.MissingDependency(ctors.Select(q => q.GetParameters()).Select(p => p.Select(r => r.ParameterType).ToArray()).ToArray());
    }

    /// <summary>
    /// Registers a lazily-instantiated service.
    /// </summary>
    /// <param name="objectType">Type of object to register.</param>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    public void Register(Type objectType, bool persistent = true)
    {
         Register(objectType, _config.TypeRegistrations(objectType), persistent);
    }

    /// <summary>
    /// Registers a lazily-instantiated service.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    /// <param name="factory">Object factory to use.</param>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    public void Register<T>(Func<T> factory, bool persistent = true) where T : notnull
    {
        Register(() => factory(), _config.TypeRegistrations(typeof(T)), persistent);
    }

    /// <summary>
    /// Registers an active sigleton instance as a service on this pool.
    /// </summary>
    /// <param name="singleton">
    /// Active instance to register.
    /// </param>
    public void RegisterNow(object singleton)
    {
        RegisterNow(singleton, _config.TypeRegistrations(singleton.GetType()));
    }

    /// <summary>
    /// Initializes all registered services marked as persistent using
    /// their respective registered factories.
    /// </summary>
    public void InitNow()
    {
        var entries = _factories.Where(p => p.Value.Persistent).ToArray();
        foreach (var entry in entries)
        {
            _factories.Remove(entry.Key);
            RegisterNow(entry.Value.Factory());
        }
    }

    /// <summary>
    /// Removes a registered service from this service pool.
    /// </summary>
    /// <param name="objectType">Type of service to remove.</param>
    /// <returns>
    /// <see langword="true"/> if a registered service matching the
    /// requested type was found and removed from the pool,
    /// <see langword="false"/> otherwise.
    /// </returns>
    public bool Remove(Type objectType)
    {
        return _instances.Remove(objectType) || _factories.Remove(objectType);
    }

    /// <summary>
    /// Registers a new instance of the specified service.
    /// </summary>
    /// <param name="singleton">Instance of the service.</param>
    /// <param name="typeRegistrations">
    /// Types to be registered as resolvable for the object to generate using
    /// the specified factory.
    /// </param>
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public void RegisterNow(object singleton, IEnumerable<Type> typeRegistrations)
    {
        CheckUniqueType(typeRegistrations);
        foreach (var j in typeRegistrations)
        {
            _instances.Add(j, singleton);
        }
    }

    /// <summary>
    /// Registers a lazily-instantiated service.
    /// </summary>
    /// <param name="factory">Object factory to use.</param>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    /// <param name="typeRegistrations">
    /// Types to be registered as resolvable for the object to generate using
    /// the specified factory.
    /// </param>
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public void Register(Func<object> factory, IEnumerable<Type> typeRegistrations, bool persistent = true)
    {
        CheckUniqueType(typeRegistrations);
        foreach (var j in typeRegistrations)
        {
            _factories.Add(j, new(persistent, factory));
        }
    }

    /// <summary>
    /// Registers a lazily-instantiated service.
    /// </summary>
    /// <param name="objectType">Type of object to register.</param>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    /// <param name="typeRegistrations">
    /// Types to be registered as resolvable for the object to generate using
    /// the specified factory.
    /// </param>
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public void Register(Type objectType, IEnumerable<Type> typeRegistrations, bool persistent = true)
    {
        Register(() => CreateInstance(objectType), typeRegistrations, persistent);
    }
    
    /// <summary>
    /// Registers a lazily-instantiated service.
    /// </summary>
    /// <typeparam name="TReg">Type of object to register.</typeparam>
    /// <typeparam name="TService">
    /// Type of the object to be instantiated upon service type request.
    /// </typeparam>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public void Register<TReg, TService>(bool persistent = true)
        where TReg : notnull
        where TService : notnull
    {
        Register<TService>([typeof(TReg)], persistent);
    }

    /// <summary>
    /// Registers a lazily-instantiated service.
    /// </summary>
    /// <typeparam name="T">Type of object to register.</typeparam>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    /// <param name="typeRegistrations">
    /// Types to be registered as resolvable for the object to generate using
    /// the specified factory.
    /// </param>
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public void Register<T>(Type[] typeRegistrations, bool persistent = true) where T : notnull
    {
        Register(typeof(T), typeRegistrations, persistent);
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator() => _instances.Values.Concat(_factories.Select(CreateFromLazy)).GetEnumerator();

    private object? ResolveLazy(Type objectType)
    {
        return _factories.TryGetValue(objectType, out FactoryEntry? factory) ? CreateFromLazy(objectType, factory) : null;
    }

    private object? ResolveActive(Type objectType)
    {
        return _instances.TryGetValue(objectType, out var service) ? service : null;
    }

    private object CreateFromLazy(KeyValuePair<Type, FactoryEntry> entry) => CreateFromLazy(entry.Key, entry.Value);

    private object CreateFromLazy(Type objectType, FactoryEntry factory)
    {
        var obj = factory.Factory.Invoke();
        if (factory.Persistent)
        {
            _factories.Remove(objectType);
            _instances.Add(objectType, obj);
        }
        return obj;
    }

    private bool IsValidCtor(ConstructorInfo ctor, Type targetType, out object[]? args)
    {
        ParameterInfo[] pars = ctor.GetParameters();
        List<object> a = [];
        foreach (ParameterInfo arg in pars)
        {
            var value =
                (targetType.IsAssignableFrom(arg.ParameterType) ? ResolveActive(arg.ParameterType) : Resolve(arg.ParameterType)) ??
                (arg.IsOptional ? Type.Missing : null);
            if (value is null) break;
            a.Add(value);
        }
        return (args = a.Count == pars.Length ? [.. a] : null) is not null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckUniqueType(IEnumerable<Type> newKeys)
    {
        var existingKeys = _instances.Keys.Concat(_factories.Keys).ToArray();
        if (newKeys.Any(existingKeys.Contains)) throw Errors.ServiceAlreadyRegistered();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<ConstructorInfo> EnumerateCtors(Type t)
    {
        return t.GetConstructors().OrderByDescending(p => p.GetParameters().Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object? CreateInstance_Internal(Type t)
    {
        foreach (ConstructorInfo ctor in EnumerateCtors(t))
        {
            if (IsValidCtor(ctor, t, out var args))
            {
                return ctor.Invoke(args);
            }
        }
        return null;
    }
}
