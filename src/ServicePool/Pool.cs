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
using System.Runtime.CompilerServices;

namespace TheXDS.ServicePool;

/// <summary>
/// Represents a collection of hosted services that can be instantiated and 
/// resolved using dependency injection, resolving objects to whichever
/// resolution type they have registered, and only allowing a single type per
/// pool to be registered.
/// </summary>
public class Pool : PoolBase
{
    /// <summary>
    /// Gets a dictionary of all factories registered to generate
    /// lazily-initialized services.
    /// </summary>
    protected Dictionary<Type, FactoryEntry> Factories { get; } = [];

    /// <summary>
    /// Gets a dictionary of all persistent singletons currently active on this
    /// pool instance.
    /// </summary>
    protected Dictionary<Type, object> Instances { get; } = [];

    /// <inheritdoc/>
    public override int Count => Instances.Count + Factories.Count;

    /// <inheritdoc/>
    public override void Register(Type objectType, bool persistent = true)
    {
         Register(objectType, [objectType], persistent);
    }

    /// <inheritdoc/>
    public override void Register<T>(Func<T> factory, bool persistent = true)
    {
        Register(() => factory(), [typeof(T)], persistent);
    }

    /// <inheritdoc/>
    public override void RegisterNow(object singleton)
    {
        RegisterNow(singleton, [singleton.GetType()]);
    }

    /// <inheritdoc/>
    public override void InitNow()
    {
        var entries = Factories.Where(p => p.Value.Persistent).ToArray();
        foreach (var entry in entries)
        {
            Factories.Remove(entry.Key);
            RegisterNow(entry.Value.Factory());
        }
    }

    /// <inheritdoc/>
    public override bool Remove(Type objectType)
    {
        return Instances.Remove(objectType) || Factories.Remove(objectType);
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
    public void RegisterNow(object singleton, Type[] typeRegistrations)
    {
        CheckUniqueType(typeRegistrations);
        foreach (var j in typeRegistrations)
        {
            Instances.Add(j, singleton);
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
    public void Register(Func<object> factory, Type[] typeRegistrations, bool persistent = true)
    {
        CheckUniqueType(typeRegistrations);
        foreach (var j in typeRegistrations)
        {
            Factories.Add(j, new(persistent, factory));
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
    public void Register(Type objectType, Type[] typeRegistrations, bool persistent = true)
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
    public override IEnumerator GetEnumerator() => Instances.Values.Concat(Factories.Select(CreateFromLazy)).GetEnumerator();

    /// <inheritdoc/>
    protected override object? ResolveLazy(Type objectType)
    {
        return Factories.TryGetValue(objectType, out FactoryEntry? factory) ? CreateFromLazy(objectType, factory) : null;
    }

    /// <inheritdoc/>
    protected override object? ResolveActive(Type objectType)
    {
        return Instances.TryGetValue(objectType, out var service) ? service : null;
    }

    private object CreateFromLazy(KeyValuePair<Type, FactoryEntry> entry) => CreateFromLazy(entry.Key, entry.Value);

    private object CreateFromLazy(Type objectType, FactoryEntry factory)
    {
        var obj = factory.Factory.Invoke();
        if (factory.Persistent)
        {
            Factories.Remove(objectType);
            Instances.Add(objectType, obj);
        }
        return obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckUniqueType(IEnumerable<Type> newKeys)
    {
        var existingKeys = Instances.Keys.Concat(Factories.Keys).ToArray();
        if (newKeys.Any(existingKeys.Contains)) throw new InvalidOperationException();
    }
}
