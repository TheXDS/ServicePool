// AssemblyListDiscoveryEngineTests.cs
//
// This file is part of ServicePool
//
// Author(s):
//      César Andrés Morgan <xds_xps_ivx@hotmail.com>
//
// Released under the MIT License (MIT)
// Copyright © 2011 - 2023 César Andrés Morgan
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

#pragma warning disable CS1591

using NUnit.Framework;
using System;
using System.Reflection;

namespace TheXDS.ServicePool.Tests;

public class AssemblyListDiscoveryEngineTests
{
    [Test]
    public void Engine_supports_add()
    {
        var a = Assembly.GetExecutingAssembly();
        var e = new AssemblyListDiscoveryEngine
        {
            a
        };
        Assert.That(e, Contains.Item(a));
    }

    [Test]
    public void Engine_supports_insert()
    {
        var a = Assembly.GetExecutingAssembly();
        var e = new AssemblyListDiscoveryEngine();
        e.Insert(0, a);
        Assert.That(e, Contains.Item(a));
    }

    [Test]
    public void Engine_supports_set()
    {
        var a = Assembly.GetExecutingAssembly();
        var b = typeof(object).Assembly;
        var e = new AssemblyListDiscoveryEngine { a };
        e[0] = b;
        Assert.That(e, Does.Not.Contain(a));
        Assert.That(e, Contains.Item(b));
    }

    [Test]
    public void Engine_add_throws_on_null()
    {
        var e = new AssemblyListDiscoveryEngine();
        Assert.Throws<ArgumentNullException>(() => e.Add(null!));
    }

    [Test]
    public void Engine_insert_throws_on_null()
    {
        var e = new AssemblyListDiscoveryEngine();
        Assert.Throws<ArgumentNullException>(() => e.Insert(0, null!));
    }

    [Test]
    public void Engine_set_throws_on_null()
    {
        var e = new AssemblyListDiscoveryEngine
        {
            Assembly.GetExecutingAssembly()
        };
        Assert.Throws<ArgumentNullException>(() => e[0] = null!);
    }
}
