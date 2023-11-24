// DefaultDiscoveryEngine.cs
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
/// Implements a <see cref="IDiscoveryEngine"/> that searches for types in 
/// the current <see cref="AppDomain"/>.
/// </summary>
/// <remarks>
/// This <see cref="IDiscoveryEngine"/> is used by default by
/// <see cref="FlexPool.Discover{T}(bool)"/> and
/// <see cref="FlexPool.DiscoverAll{T}(bool)"/>.
/// </remarks>
public class DefaultDiscoveryEngine : IDiscoveryEngine
{
    private readonly IDiscoveryEngine _engine = new AssemblyListDiscoveryEngine(AppDomain.CurrentDomain.GetAssemblies());

    /// <inheritdoc/>
    public IEnumerable<Type> Discover(Type t) => _engine.Discover(t);
}
