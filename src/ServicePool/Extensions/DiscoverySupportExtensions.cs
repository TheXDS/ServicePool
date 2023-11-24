// DiscoverySupportExtensions.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TheXDS.ServicePool.Extensions;

public static class DiscoverySupportExtensions
{
    /// <summary>
    /// Tries to resolve a registered service of type
    /// <typeparamref name="T"/>, and if not found, searches for any type
    /// in the app domain that can be instantiated and returned as the
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
    /// instantiated again.
    /// </remarks>
    public static T? Discover<T>(this PoolBase pool, bool persistent = true) where T : notnull
    {
        return (T?)Discover(pool, typeof(T), persistent);
    }

    public static object? Discover(this PoolBase pool, Type objectType, bool persistent = true)
    {
        return Discover(pool, objectType, new DefaultDiscoveryEngine(), persistent);
    }

    /// <summary>
    /// Tries to resolve a registered service of type
    /// <typeparamref name="T"/>, and if not found, searches for any type
    /// that can be instantiated and returned as the requested service.
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
    /// instantiated again.
    /// </remarks>
    public static T? Discover<T>(this PoolBase pool, IDiscoveryEngine discoveryEngine, bool persistent = true) where T : notnull
    {
        return (T?)Discover(pool, typeof(T), discoveryEngine, persistent);
    }

    public static object? Discover(this PoolBase pool, Type objectType, IDiscoveryEngine discoveryEngine, bool persistent = true)
    {
        return pool.Resolve(objectType) ?? DiscoverAll(pool, discoveryEngine, objectType, persistent).FirstOrDefault();
    }

    public static IEnumerable<object?> DiscoverAll(this PoolBase pool, IDiscoveryEngine discoveryEngine, Type t, bool persistent)
    {
        foreach (Type dt in discoveryEngine.Discover(t))
        {
            if (pool.Resolve(dt) is null && pool.CreateInstanceOrNull(dt) is { } obj)
            {
                if (persistent) pool.RegisterNow(obj);
                yield return obj;
            }
        }
    }
}

public static class SugarExtensions
{
    public static void RegisterIf<T>(this PoolBase pool, bool condition, Func<T> factory, bool persistent = true) where T : notnull
    {
        if (condition) { pool.Register(factory, persistent); }
    }
}