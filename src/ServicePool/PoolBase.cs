// FlexPoolBase.cs
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
    /// <param name="Persistent">If set to <see langword="true"/>, once the object instance is created, the new object will be persisted inside the pool and its factory removed.</param>
    /// <param name="Factory"></param>
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
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public void Register<T>(bool persistent = true) where T : notnull => Register(typeof(T), persistent);

    /// <summary>
    /// Instances and registers a new service of type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public void RegisterNow<T>(T singleton) where T : notnull => RegisterNow(singleton);

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
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
    public abstract void Register<T>(Func<T> factory, bool persistent = true) where T : notnull;
    
    public abstract void RegisterNow(object singleton);

    /// <summary>
    /// Initializes all registered services marked as persistent using
    /// their respective registered factories.
    /// </summary>
    /// <returns>
    /// This same service pool instance, allowing the use of Fluent syntax.
    /// </returns>
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

    public object CreateInstance(Type t)
    {
        return CreateInstanceOrNull(t) ?? throw Errors.CantInstantiate();
    }

    protected abstract object? ResolveLazy(Type objectType);

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