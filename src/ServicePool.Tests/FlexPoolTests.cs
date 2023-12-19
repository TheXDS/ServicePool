// FlexPoolTests.cs
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

#pragma warning disable CS1591

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TheXDS.ServicePool.Extensions;
using TheXDS.ServicePool.TestTypes;

namespace TheXDS.ServicePool.Tests;

public class FlexPoolTests : PoolBaseTests<FlexPool>
{
    [Test]
    public void Resolve_resolves_for_interfaces()
    {
        FlexPool? pool = new();
        pool.Register<Test1>();
        Assert.That(pool.Resolve<ITest>(), Is.Not.Null);
    }

    [Test]
    public void Resolve_resolves_for_base_class()
    {
        FlexPool? pool = new();
        pool.Register<Test3>();
        Assert.That(pool.Resolve<Test1>(), Is.Not.Null);
    }

    [Test]
    public void Discover_searches_for_service()
    {
        FlexPool? pool = new();
        Assert.That(pool.Discover<Random>(), Is.InstanceOf<Random>());
        Assert.That(pool.Count, Is.EqualTo(1));
    }

    [Test]
    public void Discover_returns_from_active_services()
    {
        FlexPool? pool = new();
        pool.Register<Random>();
        var r = pool.Resolve<Random>();
        Assert.That(pool.Discover<Random>(), Is.SameAs(r));
    }

    [Test]
    public void ResolveAll_returns_collection()
    {
        FlexPool pool = new();
        pool.RegisterNow(new List<int>());
        pool.RegisterNow(new Collection<int>());
        pool.RegisterNow(Array.Empty<int>());
        Assert.That(pool.ResolveAll<IEnumerable<int>>().Count(), Is.EqualTo(3));
    }

    [Test]
    public void DiscoverAll_enumerates_all_types_that_implement_base_type()
    {
        FlexPool pool = new();
        Assert.That(pool.DiscoverAll<ITest>().ToArray().Length, Is.EqualTo(3));
    }

    [Test]
    public void DiscoverAll_skips_existing_services()
    {
        FlexPool pool = new();
        pool.RegisterNow<Test1>();
        var t1 = pool.Resolve<Test1>();
        ITest[] c = pool.DiscoverAll<ITest>().ToArray();
        Assert.That(c.Length, Is.EqualTo(3));
        Assert.That(c[0], Is.SameAs(t1));
        Assert.That(c[1], Is.InstanceOf<Test2>());
        Assert.That(c[2], Is.InstanceOf<Test3>());
    }

    [Test]
    public void DiscoverAll_allows_specifying_engine()
    {
        FlexPool pool = new();
        ITest[] c = pool.DiscoverAll<ITest>(new DefaultDiscoveryEngine()).ToArray();
        Assert.That(c.Length, Is.EqualTo(3));        
    }

    [Test]
    public void Discover_allows_specifying_engine()
    {
        FlexPool pool = new();
        var c = pool.Discover<ITest>(new DefaultDiscoveryEngine());
        Assert.That(c, Is.Not.Null);
    }
}
