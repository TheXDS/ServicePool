// FlexPool.cs
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

namespace TheXDS.ServicePool;

/// <summary>
/// Represents a collection of hosted services that can be instantiated and 
/// resolved using dependency injection, resolving objects to any implemented
/// interface/base type defined in their declaration without explicit
/// registration.
/// </summary>
public class FlexPool : PoolBase
{
    private record FlexFactoryEntry(in Type Type, in bool Persistent, in Func<object> Factory) : FactoryEntry(Persistent, Factory);

    private readonly List<FlexFactoryEntry> _factories = [];
    private readonly List<object> _instances = [];

    /// <inhertdoc/>
    public override int Count => _instances.Count + _factories.Count;

    /// <inhertdoc/>
    public override void Register(Type objectType, bool persistent = true)
    {
        _factories.Add(new(objectType, persistent, () => CreateInstance(objectType)));
    }

    /// <inhertdoc/>
    public override void Register<T>(Func<T> factory, bool persistent = true)
    {
        _factories.Add(new(typeof(T), persistent, () => factory()));
    }

    /// <inhertdoc/>
    public override void InitNow()
    {
        var entries = _factories.Where(p => p.Persistent).ToArray();
        foreach (var entry in entries)
        {
            RegisterNow(entry.Factory());
            _factories.Remove(entry);
        }
    }

    /// <inhertdoc/>
    public override IEnumerator GetEnumerator()
    {
        return _instances.ToArray().Concat(_factories.Select(CreateFromLazy).ToArray()).GetEnumerator();
    }

    /// <inhertdoc/>
    public override void RegisterNow(object singleton)
    {
        _instances.Add(singleton);
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
        return _instances.Remove(service);
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
    public override bool Remove(Type objectType)
    {
        return ResolveActive(objectType) is { } o ? Remove(o) : (GetLazyFactory(objectType).FirstOrDefault() is { } f && _factories.Remove(f));
    }

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
        return _instances.OfType<T>()
            .Concat(GetLazyFactory(typeof(T))
            .Select(CreateFromLazy).Cast<T>());
    }

    /// <inhertdoc/>
    protected override object? ResolveActive(Type serviceType)
    {
        return _instances.FirstOrDefault(p => serviceType.IsAssignableFrom(p.GetType()));
    }

    /// <inhertdoc/>
    protected override object? ResolveLazy(Type objectType)
    {
        return GetLazyFactory(objectType).FirstOrDefault() is { } f
            ? CreateFromLazy(f)
            : null;
    }

    private object CreateFromLazy(FlexFactoryEntry factory)
    {
        var obj = factory.Factory.Invoke();
        if (factory.Persistent)
        {
            _factories.Remove(factory);
            _instances.Add(obj);
        }
        return obj;
    }

    private IEnumerable<FlexFactoryEntry> GetLazyFactory(Type serviceType)
    {
        return _factories.Where(p => serviceType.IsAssignableFrom(p.Type));
    }
}
