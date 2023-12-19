// PoolBase.cs
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
using TheXDS.ServicePool.Resources;

namespace TheXDS.ServicePool;

/// <summary>
/// Base class for all service pools that share dependency resolution logic
/// using object factories.
/// </summary>
public abstract class PoolBase : IEnumerable
{
    /// <summary>
    /// Represents a single lazily-initialized object factory item.
    /// </summary>
    /// <param name="Persistent">
    /// If set to <see langword="true"/>, once the object instance is created,
    /// the new object will be persisted inside the pool and its factory
    /// removed.
    /// </param>
    /// <param name="Factory">
    /// Factory to use when creating a new instance of the requested service.
    /// </param>
    protected record FactoryEntry(in bool Persistent, in Func<object> Factory);

    /// <summary>
    /// Gets a count of all the registered services in this pool.
    /// </summary>
    public abstract int Count { get; }

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
    /// Instances and registers a service of type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <param name="singleton">Singleton instance to register.</param>
    /// <typeparam name="T">Type of service to register.</typeparam>
    public void RegisterNow<T>(T singleton) where T : notnull => RegisterNow((object)singleton);

    /// <summary>
    /// Instances and registers a new service of type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    public void RegisterNow<T>() where T : notnull => RegisterNow(CreateInstance(typeof(T)));

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
    public abstract void Register(Type objectType, bool persistent = true);

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
    public abstract void Register<T>(Func<T> factory, bool persistent = true) where T : notnull;

    /// <summary>
    /// Registers an active sigleton instance as a service on this pool.
    /// </summary>
    /// <param name="singleton">
    /// Active instance to register.
    /// </param>
    public abstract void RegisterNow(object singleton);

    /// <summary>
    /// Initializes all registered services marked as persistent using
    /// their respective registered factories.
    /// </summary>
    public abstract void InitNow();

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
    /// Removes a registered service from this service pool.
    /// </summary>
    /// <param name="objectType">Type of service to remove.</param>
    /// <returns>
    /// <see langword="true"/> if a registered service matching the
    /// requested type was found and removed from the pool,
    /// <see langword="false"/> otherwise.
    /// </returns>
    public abstract bool Remove(Type objectType);

    /// <inheritdoc/>
    public abstract IEnumerator GetEnumerator();

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
        ConstructorInfo[] ctors = [.. t.GetConstructors().OrderByDescending(p => p.GetParameters().Length)];
        foreach (ConstructorInfo ctor in ctors)
        {
            if (IsValidCtor(ctor, t, out var args))
            {
                return ctor.Invoke(args);
            }
        }
        return null;
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
        return CreateInstanceOrNull(t) ?? throw Errors.CantInstantiate();
    }

    /// <summary>
    /// Defines the resolution logic to be used when requesting a service that
    /// should be initialized from a factory.
    /// </summary>
    /// <param name="objectType">Requested service type.</param>
    /// <returns>
    /// The resolved object instance, or <see langword="null"/> if the service
    /// could not be lazily resolved.
    /// </returns>
    protected abstract object? ResolveLazy(Type objectType);

    /// <summary>
    /// Defines the resolution logic to be used when requesting a service that
    /// should be returned from a collection of active object instances.
    /// </summary>
    /// <param name="objectType">Requested service type.</param>
    /// <returns>
    /// The resolved object instance, or <see langword="null"/> if there is no
    /// oject that could be mapped to the requested type.
    /// </returns>
    protected abstract object? ResolveActive(Type objectType);

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
}