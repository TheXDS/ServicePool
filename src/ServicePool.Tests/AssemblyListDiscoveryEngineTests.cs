// AssemblyListDiscoveryEngineTests.cs
//
// This file is part of ServicePool
//
// Author(s):
//      César Andrés Morgan <xds_xps_ivx@hotmail.com>
//
// Copyright (C) 2021 César Andrés Morgan
//
// ServicePool is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later
// version.
//
// ServicePool is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY, without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// this program. If not, see <http://www.gnu.org/licenses/>.

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
        Assert.Contains(a, e);
    }

    [Test]
    public void Engine_supports_insert()
    {
        var a = Assembly.GetExecutingAssembly();
        var e = new AssemblyListDiscoveryEngine();
        e.Insert(0, a);
        Assert.Contains(a, e);
    }

    [Test]
    public void Engine_supports_set()
    {
        var a = Assembly.GetExecutingAssembly();
        var b = typeof(object).Assembly;
        var e = new AssemblyListDiscoveryEngine { a };
        e[0] = b;
        Assert.False(e.Contains(a));
        Assert.Contains(b, e);
    }

    [Test]
    public void Engine_add_throws_on_null()
    {
        var a = Assembly.GetExecutingAssembly();
        var e = new AssemblyListDiscoveryEngine();
        Assert.Throws<ArgumentNullException>(() => e.Add(null!));
    }

    [Test]
    public void Engine_insert_throws_on_null()
    {
        var a = Assembly.GetExecutingAssembly();
        var e = new AssemblyListDiscoveryEngine();
        Assert.Throws<ArgumentNullException>(() => e.Insert(0, null!));
    }

    [Test]
    public void Engine_set_throws_on_null()
    {
        var a = Assembly.GetExecutingAssembly();
        var e = new AssemblyListDiscoveryEngine
        {
            Assembly.GetExecutingAssembly()
        };
        Assert.Throws<ArgumentNullException>(() => e[0] = null!);
    }
}
