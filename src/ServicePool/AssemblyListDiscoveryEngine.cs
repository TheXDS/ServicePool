// AssemblyListDiscoveryEngine.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace TheXDS.ServicePool;

/// <summary>
/// Implements a <see cref="IDiscoveryEngine"/> that searches for types in
/// an internal list of assemblies that have been previously registered.
/// </summary>
public class AssemblyListDiscoveryEngine : Collection<Assembly>, IDiscoveryEngine
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssemblyListDiscoveryEngine"/> class.
    /// </summary>
    public AssemblyListDiscoveryEngine()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssemblyListDiscoveryEngine"/> class.
    /// </summary>
    /// <param name="list">
    /// The list that is wrapped by the new collection.
    /// </param>
    public AssemblyListDiscoveryEngine(IList<Assembly> list) : base(list)
    {
    }

    /// <inheritdoc/>
    protected override void InsertItem(int index, Assembly item)
    {
        base.InsertItem(index, item ?? throw new ArgumentNullException(nameof(item)));
    }

    /// <inheritdoc/>
    protected override void SetItem(int index, Assembly item)
    {
        base.SetItem(index, item ?? throw new ArgumentNullException(nameof(item)));
    }

    /// <inheritdoc/>
    public IEnumerable<Type> Discover(Type t)
    {
        return this.SelectMany(p => p.GetTypes())
            .Where(p => !p.IsAbstract && !p.IsInterface && t.IsAssignableFrom(p));
    }
}
