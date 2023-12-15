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

namespace TheXDS.ServicePool.Extensions;

/// <summary>
/// Includes a set of extensions to extend the functionality of service pools
/// to include quick registration and/or consumption methods.
/// </summary>
public static class SugarExtensions
{
    /// <summary>
    /// Registers a lazily-instantiated service if the specified condition
    /// resolves to <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    /// <param name="pool">Pool to register the service into.</param>
    /// <param name="condition">
    /// If this value evals to <see langword="true"/>, the service will be
    /// registered; otherwise no action is taken.
    /// </param>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    public static void RegisterIf<T>(this PoolBase pool, bool condition, bool persistent = true) where T : notnull
    {
        if (condition) pool.Register<T>(persistent);
    }

    /// <summary>
    /// Registers a lazily-instantiated service if the specified condition
    /// resolves to <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    /// <param name="pool">Pool to register the service into.</param>
    /// <param name="condition">
    /// If this value evals to <see langword="true"/>, the service will be
    /// registered; otherwise no action is taken.
    /// </param>
    /// <param name="factory">Object factory to use.</param>
    /// <param name="persistent">
    /// If set to <see langword="true"/>, the resolved object is going to be
    /// persisted in the service pool (it will be a Singleton). When 
    /// <see langword="false"/>, the registered service will be instantiated
    /// and initialized each time it is requested (it will be transient).
    /// </param>
    public static void RegisterIf<T>(this PoolBase pool, bool condition, Func<T> factory, bool persistent = true) where T : notnull
    {
        if (condition) pool.Register(factory, persistent);
    }

    /// <summary>
    /// Registers a singleton instance if the specified condition resolves to
    /// <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    /// <param name="pool">Pool to register the service into.</param>
    /// <param name="condition">
    /// If this value evals to <see langword="true"/>, the service will be
    /// registered; otherwise no action is taken.
    /// </param>
    /// <param name="singleton">Object instance to register.</param>
    public static void RegisterNowIf<T>(this PoolBase pool, bool condition, T singleton) where T : notnull
    {
        if (condition) pool.RegisterNow(singleton);
    }

    /// <summary>
    /// Registers a singleton instance if the specified condition resolves to
    /// <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Type of service to register.</typeparam>
    /// <param name="pool">Pool to register the service into.</param>
    /// <param name="condition">
    /// If this value evals to <see langword="true"/>, the service will be
    /// registered; otherwise no action is taken.
    /// </param>
    public static void RegisterNowIf<T>(this PoolBase pool, bool condition) where T : notnull
    {
        if (condition) pool.RegisterNow<T>();
    }
}