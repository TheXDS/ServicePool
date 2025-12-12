// FilterableDiscoveryEngine.cs
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

namespace TheXDS.ServicePool;

/// <summary>
/// Implements a <see cref="IDiscoveryEngine"/> that searches for types in the
/// current <see cref="AppDomain"/>, but excludes the types explicitly added to
/// a list of exclusions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="FilterableDiscoveryEngine"/> class.
/// </remarks>
/// <param name="exclusions">
/// Types to be excluded from the search when discovering types.
/// </param>
public class FilterableDiscoveryEngine(IEnumerable<Type>? exclusions) : IDiscoveryEngine
{
    private readonly HashSet<Type> _exclusions = [.. exclusions ?? []];
    private readonly IDiscoveryEngine _engine = new DefaultDiscoveryEngine();

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="FilterableDiscoveryEngine"/> class.
    /// </summary>
    public FilterableDiscoveryEngine() : this((IEnumerable<Type>?)null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="FilterableDiscoveryEngine"/> class.
    /// </summary>
    /// <param name="exclusions">
    /// Types to be excluded from the search when discovering types.
    /// </param>
    public FilterableDiscoveryEngine(params Type[] exclusions) : this(exclusions.AsEnumerable()) { }

    /// <summary>
    /// Gets a collection of types that should be omitted from being
    /// automatically discovered.
    /// </summary>
    public ICollection<Type> Exclusions => _exclusions;

    /// <inheritdoc/>
    public IEnumerable<Type> Discover(Type t) => _engine.Discover(t).Where(p => !_exclusions.Contains(p));
}