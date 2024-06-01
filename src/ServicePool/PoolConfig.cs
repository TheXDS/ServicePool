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
using System.Collections.Generic;

namespace TheXDS.ServicePool;

/// <summary>
/// Contains a set of configuration values to be used when instanciating a new
/// <see cref="Pool"/>.
/// </summary>
public readonly record struct PoolConfig
{
    /// <summary>
    /// Defines the logic to use when enumerating the types to register a
    /// specific service type for.
    /// </summary>
    public required Func<Type, IEnumerable<Type>> TypeRegistrations { get; init; }

    /// <summary>
    /// Represents the default configuration for a <see cref="Pool"/>.
    /// </summary>
    public static readonly PoolConfig Default = new() 
    {
        TypeRegistrations = t => [t]
    };

    /// <summary>
    /// Represents the configuration to use for a <see cref="Pool"/> that can
    /// resolve a single service type based on its entire inheritance tree and
    /// all the interfaces it implements.
    /// </summary>
    public static readonly PoolConfig Flex = new() {
        TypeRegistrations = t => [t, ..GetBaseTypes(t), ..t.GetInterfaces()]
    };

    private static IEnumerable<Type> GetBaseTypes(Type t)
    {
        var i = t.BaseType;
        while (i is not null)
        {
            yield return i;
            i = i.BaseType;
        }
    }
}
