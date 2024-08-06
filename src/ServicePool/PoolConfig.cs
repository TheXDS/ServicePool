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
using System.Reflection;
using System.Linq;

namespace TheXDS.ServicePool;

/// <summary>
/// Contains a set of configuration values to be used when instanciating a new
/// <see cref="Pool"/>.
/// </summary>
public readonly record struct PoolConfig
{
    /// <summary>
    /// Defines the logic to use when resolving a dependency for a service that
    /// needs to be instanced.
    /// </summary>
    public required Func<Pool, Type, object?> DependencyResolver { get; init; }

    /// <summary>
    /// Gets a value that indicates if the <see cref="Pool"/> itself can be
    /// resolved as a service.
    /// </summary>
    public required bool SelfRegister { get; init; }

    /// <summary>
    /// Defines the logic to use when enumerating the types to register a
    /// specific service type for.
    /// </summary>
    public required Func<Type, IEnumerable<Type>> TypeRegistrations { get; init; }

    /// <summary>
    /// Defines the logic to be used when trying to resolve a service type from
    /// the Factory entry dictionary.
    /// </summary>
    public required Func<Type, IDictionary<Type, Pool.FactoryEntry>, Pool.FactoryEntry?> FactoryResolver { get; init; }

    /// <summary>
    /// Defines the logic to be used when trying to resolve a service type from
    /// the active service collection.
    /// </summary>
    public required Func<Type, IDictionary<Type, object>, object?> ActiveResolver { get; init; }

    /// <summary>
    /// Gets a reference to the <see cref="IDiscoveryEngine"/> to use when the
    /// user requests the pool to discover services.
    /// </summary>
    public required IDiscoveryEngine DiscoveryEngine { get; init; }

    /// <summary>
    /// Defines the logic to use when enumerating the constructors available
    /// for a type when resolving dependencies.
    /// </summary>
    public required Func<Type, IEnumerable<ConstructorInfo>> ConstructorEnumeration { get; init; }

    /// <summary>
    /// Represents the default configuration for a <see cref="Pool"/>.
    /// </summary>
    public static readonly PoolConfig Default = new() 
    {
        DependencyResolver = (p, t) => p.Resolve(t),
        SelfRegister = true,
        TypeRegistrations = t => [t],
        ActiveResolver = (t, d) => d.TryGetValue(t, out var v) ? v : null,
        FactoryResolver = (t, d) => d.TryGetValue(t, out var v) ? v : null,
        DiscoveryEngine = new DefaultDiscoveryEngine(),
        ConstructorEnumeration = (t) => t.GetConstructors().OrderByDescending(p => p.GetParameters().Length)
    };

    /// <summary>
    /// Represents the configuration to use for a <see cref="Pool"/> that can
    /// resolve a service if it can be assigned to the requested service type.
    /// </summary>
    /// <remarks>
    /// While resolving services using either <see cref="FlexResolve"/> or
    /// <see cref="FlexRegister"/> generally works the same, semantics between
    /// them vary in the kind of services that could be registered.
    /// Using <see cref="FlexResolve"/> may allow multiple services that can
    /// expose the same service type to be registered, but only the first one
    /// to match will be resolved.
    /// </remarks>
    /// /// <seealso cref="FlexRegister"/>
    public static readonly PoolConfig FlexResolve = Default with {
        ActiveResolver = (t, d) => d.Keys.FirstOrDefault(p => p.IsAssignableTo(t)) is {} k ? d[k] : null,
        FactoryResolver = (t, d) => d.Keys.FirstOrDefault(p => p.IsAssignableTo(t)) is {} k ? d[k] : null
    };

    /// <summary>
    /// Represents the configuration to use for a <see cref="Pool"/> that can
    /// register a single service type based on its entire inheritance tree and
    /// all the interfaces it implements.
    /// </summary>
    /// <remarks>
    /// While resolving services using either <see cref="FlexResolve"/> or
    /// <see cref="FlexRegister"/> generally works the same, semantics between
    /// them vary in the kind of services that could be registered.
    /// Using <see cref="FlexRegister"/> will register the specified service
    /// type, as well as all of its base classes (except <see cref="object"/>)
    /// and implemented interfaces.
    /// </remarks>
    /// <seealso cref="FlexResolve"/>
    public static readonly PoolConfig FlexRegister = Default with {
        TypeRegistrations = t => [t, ..GetBaseTypes(t), ..t.GetInterfaces()],
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
