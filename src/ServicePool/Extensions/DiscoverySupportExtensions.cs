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

/// <summary>
/// Includes a set of extensions to extend the functionality of service pools
/// to include service discovery functionality.
/// </summary>
public static class DiscoverySupportExtensions
{
    /// <summary>
    /// Tries to resolve a registered service of type
    /// <typeparamref name="T"/>, and if not found, searches for any type
    /// in the app domain that can be instantiated and returned as the
    /// requested service.
    /// </summary>
    /// <typeparam name="T">Type of service to get.</typeparam>
    /// <param name="pool">
    /// Pool instance to discover the services from/onto.
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
    public static T? Discover<T>(this Pool pool) where T : notnull
    {
        return (T?)Discover(pool, typeof(T));
    }

    /// <summary>
    /// Tries to resolve a registered service of the specified type, and if not
    /// found, searches for any type that can be instantiated and returned as
    /// the requested service.
    /// </summary>
    /// <param name="pool">
    /// Pool instance to discover the service from/onto.
    /// </param>
    /// <param name="objectType">Type fo service to discover.</param>
    /// <returns>
    /// A registered service or a newly discovered one if it implements the
    /// requested type, or <see langword="null"/> in case that no
    /// discoverable service for the requested type exists.
    /// </returns>
    public static object? Discover(this Pool pool, Type objectType)
    {
        return Discover(pool, objectType, new DefaultDiscoveryEngine());
    }

    /// <summary>
    /// Tries to resolve a registered service of type
    /// <typeparamref name="T"/>, and if not found, searches for any type
    /// that can be instantiated and returned as the requested service.
    /// </summary>
    /// <typeparam name="T">Type of service to get.</typeparam>
    /// <param name="pool">
    /// Pool instance to discover the service from/onto.
    /// </param>
    /// <param name="discoveryEngine">
    /// Discovery engine to use while searching for new instantiable types.
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
    public static T? Discover<T>(this Pool pool, IDiscoveryEngine discoveryEngine) where T : notnull
    {
        return (T?)Discover(pool, typeof(T), discoveryEngine);
    }

    /// <summary>
    /// Tries to resolve a registered service of the specified type, and if not
    /// found, searches for any type that can be instantiated and returned as
    /// the requested service.
    /// </summary>
    /// <param name="pool">
    /// Pool instance to discover the service from/onto.
    /// </param>
    /// <param name="objectType">Type fo service to discover.</param>
    /// <param name="discoveryEngine">
    /// Discovery engine to use while searching for new instantiable types.
    /// </param>
    /// <returns>
    /// A registered service or a newly discovered one if it implements the
    /// requested type, or <see langword="null"/> in case that no
    /// discoverable service for the requested type exists.
    /// </returns>
    public static object? Discover(this Pool pool, Type objectType, IDiscoveryEngine discoveryEngine)
    {
        return pool.Resolve(objectType) ?? DiscoverAll(pool, discoveryEngine, objectType).FirstOrDefault();
    }

    /// <summary>
    /// Discovers all the available service instances that implement the specified service type.
    /// </summary>
    /// <param name="pool">
    /// Pool instance to discover the services from/onto.
    /// </param>
    /// <param name="discoveryEngine">
    /// Discovery engine to use while searching for new instantiable types.
    /// </param>
    /// <param name="t">Type of service to discover.</param>
    /// <returns>
    /// An enumeration of all discovered services.
    /// </returns>
    public static IEnumerable<object?> DiscoverAll(this Pool pool, IDiscoveryEngine discoveryEngine, Type t)
    {
        foreach (Type dt in discoveryEngine.Discover(t))
        {
            if (pool.Resolve(dt) is null && pool.CreateInstanceOrNull(dt) is { } obj)
            {
                yield return obj;
            }
        }
    }

    /// <summary>
    /// Tries to resolve and register all services of type
    /// <typeparamref name="T"/> found in the current app domain, returning
    /// the resulting enumeration of all services found.
    /// </summary>
    /// <typeparam name="T">Type of service to get.</typeparam>
    /// <param name="pool">
    /// Pool instance to discover the services from/onto.
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
    public static IEnumerable<T> DiscoverAll<T>(this Pool pool) where T : notnull
    {
        return DiscoverAll<T>(pool, new DefaultDiscoveryEngine());
    }

    /// <summary>
    /// Tries to resolve and register all services of type
    /// <typeparamref name="T"/> found using the specified
    /// <see cref="IDiscoveryEngine"/>, returning the resulting enumeration
    /// of all services found.
    /// </summary>
    /// <typeparam name="T">Type of service to get.</typeparam>
    /// <param name="pool">
    /// Pool instance to discover the services from/onto.
    /// </param>
    /// <param name="discoveryEngine">
    /// Discovery engine to use while searching for new instantiable types.
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
    public static IEnumerable<T> DiscoverAll<T>(this Pool pool, IDiscoveryEngine discoveryEngine) where T : notnull
    {
        return ((T?[])[pool.Resolve<T>(), .. DiscoverAll(pool, discoveryEngine, typeof(T)).Cast<T>()]).Where(p => p is not null)!;
    }
}
