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
using TheXDS.ServicePool.TestTypes;

namespace TheXDS.ServicePool.Tests;

public class PoolTests : PoolBaseTests<Pool>
{
    [Test]
    public void Resolve_resolves_for_interfaces()
    {
        Pool? pool = new();
        pool.Register<ITest, Test1>();
        Assert.That(pool.Resolve<ITest>(), Is.Not.Null);
    }

    [Test]
    public void Resolve_resolves_for_base_class()
    {
        Pool? pool = new();
        pool.Register<Test1, Test3>();
        Assert.That(pool.Resolve<Test1>(), Is.Not.Null);
    }

    [TestCase(typeof(ITest))]
    [TestCase(typeof(Test1))]
    [TestCase(typeof(Test3))]
    public void Resolve_resolves_for_various_types(Type resolvableType)
    {
        Pool? pool = new();
        pool.Register<Test3>([typeof(ITest), typeof(Test1), typeof(Test3)]);
        Assert.That(pool.Resolve(resolvableType), Is.Not.Null);
    }

    [Test]
    public void Resolve_returns_null_if_not_explicitly_registered_for_interface()
    {
        Pool? pool = new();
        pool.Register<Test1>();
        Assert.That(pool.Resolve<ITest>(), Is.Null);
    }

    [Test]
    public void Resolve_returns_null_if_not_explicitly_registered_for_own_type()
    {
        Pool? pool = new();
        pool.Register<ITest, Test1>();
        Assert.That(pool.Resolve<Test1>(), Is.Null);
    }
}
