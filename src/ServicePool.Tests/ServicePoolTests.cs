// ServicePoolTests.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace TheXDS.ServicePool.Tests;

public class ServicePoolTests
{
    [Test]
    public void Registration_api_is_fluent()
    {
        ServicePool? pool = new();
        Assert.AreSame(pool, pool.Register<Random>());
        Assert.AreSame(pool, pool.Register(DummyFactory));
        Assert.AreSame(pool, pool.InitNow());
        Assert.AreSame(pool, pool.RegisterNow<EventArgs>());
        Assert.AreSame(pool, pool.RegisterNow(new object()));
        Assert.AreSame(pool, pool.RegisterIf<Guid>(false));
        Assert.AreSame(pool, pool.RegisterIf(false, DummyFactory));
        Assert.AreSame(pool, pool.RegisterNowIf<EventArgs>(false));
        Assert.AreSame(pool, pool.RegisterNowIf(false, new object()));
    }

    [Test]
    public void ActiveCount_gets_correct_count()
    {
        ServicePool? pool = new();
        Assert.Zero(pool.ActiveCount);

        pool.RegisterNow<Exception>();
        Assert.AreEqual(1, pool.ActiveCount);

        pool.RegisterNow<EventArgs>();
        Assert.AreEqual(2, pool.ActiveCount);

        pool.Register<Random>();
        Assert.AreEqual(2, pool.ActiveCount);
    }

    [Test]
    public void Count_gets_total_service_count()
    {
        ServicePool? pool = new();
        Assert.Zero(pool.Count);

        pool.RegisterNow<Exception>();
        Assert.AreEqual(1, pool.Count);

        pool.RegisterNow<EventArgs>();
        Assert.AreEqual(2, pool.Count);

        pool.Register<Random>();
        Assert.AreEqual(3, pool.Count);
    }

    [Test]
    public void Pool_registers_and_resolves_cached_services()
    {
        ServicePool? pool = new ServicePool().RegisterNow<Random>();
        Assert.IsInstanceOf<Random>(pool.Resolve<Random>());
    }

    [Test]
    public void Pool_resolves_persistent_services()
    {
        ServicePool? pool = new ServicePool().Register<Random>(true);
        Random? r1 = pool.Resolve<Random>();
        Random? r2 = pool.Resolve<Random>();

        Assert.IsInstanceOf<Random>(r1);
        Assert.IsInstanceOf<Random>(r2);
        Assert.AreSame(r1, r2);
    }

    [Test]
    public void Pool_resolves_non_persistent_services()
    {
        ServicePool? pool = new ServicePool().Register<Random>(false);
        Random? r1 = pool.Resolve<Random>();
        Random? r2 = pool.Resolve<Random>();

        Assert.IsInstanceOf<Random>(r1);
        Assert.IsInstanceOf<Random>(r2);
        Assert.AreNotSame(r1, r2);
    }

    [Test]
    public void Resolve_returns_null_if_not_registered()
    {
        ServicePool? pool = new();
        Assert.IsNull(pool.Resolve<Random>());
    }

    [Test]
    public void Resolve_resolves_for_interfaces()
    {
        ServicePool? pool = new();
        pool.Register<Test1>();
        Assert.IsNotNull(pool.Resolve<ITest>());
    }

    [Test]
    public void Resolve_resolves_for_base_class()
    {
        ServicePool? pool = new();
        pool.Register<Test3>();
        Assert.IsNotNull(pool.Resolve<Test1>());
    }

    [Test]
    public void Pool_inits_lazy_services_on_InitNow()
    {
        ServicePool? pool = new ServicePool().Register<Random>();
        Assert.Zero(pool.ActiveCount);
        pool.InitNow();
        Assert.AreEqual(1, pool.ActiveCount);
    }

    [Test]
    public void InitNow_skips_non_persistent_services()
    {
        ServicePool? pool = new ServicePool().Register<Random>(false);
        Assert.Zero(pool.ActiveCount);
        pool.InitNow();
        Assert.Zero(pool.ActiveCount);
    }

    [Test]
    public void RegisterIf_skips_if_false()
    {
        ServicePool? pool = new ServicePool().RegisterIf<Random>(false);
        Assert.Zero(pool.Count);
        pool.RegisterIf<Random>(true);
        Assert.AreEqual(1, pool.Count);
        pool.RegisterIf(false, DummyFactory);
        Assert.AreEqual(1, pool.Count);
        pool.RegisterIf(true, DummyFactory);
        Assert.AreEqual(2, pool.Count);
    }

    [Test]
    public void RegisterNowIf_skips_if_false()
    {
        ServicePool? pool = new ServicePool().RegisterNowIf<Random>(false);
        Assert.Zero(pool.Count);
        pool.RegisterNowIf<Random>(true);
        Assert.AreEqual(1, pool.Count);
        pool.RegisterNowIf(false, new Exception());
        Assert.AreEqual(1, pool.Count);
        pool.RegisterNowIf(true, new Exception());
        Assert.AreEqual(2, pool.Count);
    }

    [Test]
    public void Discover_searches_for_service()
    {
        ServicePool? pool = new();
        Assert.IsInstanceOf<Random>(pool.Discover<Random>());
        Assert.AreEqual(1, pool.Count);
    }

    [Test]
    public void Discover_returns_from_active_services()
    {
        ServicePool? pool = new();
        pool.Register<Random>();
        var r = pool.Resolve<Random>();
        Assert.AreSame(r, pool.Discover<Random>());
    }

    [Test]
    public void ResolveAll_returns_collection()
    {
        ServicePool pool = new();
        pool.RegisterNow(new List<int>());
        pool.RegisterNow(new Collection<int>());
        pool.RegisterNow(Array.Empty<int>());
        Assert.AreEqual(3, pool.ResolveAll<IEnumerable<int>>().Count());
    }

    [Test]
    public void DiscoverAll_enumerates_all_types_that_implement_base_type()
    {
        ServicePool pool = new();
        Assert.AreEqual(3, pool.DiscoverAll<ITest>().ToArray().Length);
    }

    [Test]
    public void DiscoverAll_skips_existing_services()
    {
        ServicePool pool = new();
        pool.RegisterNow<Test1>();
        var t1 = pool.Resolve<Test1>();
        ITest[] c = pool.DiscoverAll<ITest>().ToArray();
        Assert.AreEqual(3, c.Length);
        Assert.AreSame(t1, c[0]);
        Assert.IsInstanceOf<Test2>(c[1]);
        Assert.IsInstanceOf<Test3>(c[2]);
    }

    [Test]
    public void Pool_supports_removal()
    {
        ServicePool pool = new();
        pool.RegisterNow<Test1>();
        pool.Register<Test2>();
        Assert.IsTrue(pool.Remove<Test1>());
        Assert.AreEqual(1, pool.Count);
        Assert.IsFalse(pool.Remove<Test1>());
        Assert.IsTrue(pool.Remove<Test2>());
        Assert.Zero(pool.Count);
        Assert.IsFalse(pool.Remove<Test2>());
    }

    [Test]
    public void Consume_removes_service()
    {
        ServicePool pool = new();
        pool.RegisterNow<Test1>();
        Assert.IsInstanceOf<Test1>(pool.Consume<Test1>());
        Assert.Zero(pool.Count);
        Assert.Null(pool.Consume<Test1>());
        pool.Register<Test1>();
        Assert.IsInstanceOf<Test1>(pool.Consume<Test1>());
        Assert.Zero(pool.Count);
        Assert.Null(pool.Consume<Test1>());
    }

    [Test]
    public void Enumerator_includes_lazy_and_eager_items()
    {
        ServicePool pool = new();
        pool.RegisterNow<Test1>().Register<Test2>();

        IEnumerator e = pool.GetEnumerator();
        Assert.IsInstanceOf<IEnumerator>(e);
        int c = 0;
        while (e.MoveNext())
        {
            Assert.IsInstanceOf<ITest>(e.Current);
            c++;
        }
        Assert.AreEqual(2, c);
    }

    [Test]
    public void Common_ServicePool_exists()
    {
        Assert.IsAssignableFrom<ServicePool>(ServicePool.CommonPool);
        ServicePool pool = ServicePool.CommonPool;
        Assert.AreSame(pool, ServicePool.CommonPool);
    }

    [Test]
    public void ServicePool_throws_error_on_invalid_registrations()
    {
        ServicePool pool = new();
        Assert.Catch<InvalidOperationException>(() => pool.RegisterNow<ITest>());
        Assert.Catch<InvalidOperationException>(() => pool.RegisterNow<AbstractTest>());
    }

    [Test]
    public void ServicePool_errors_have_messages()
    {
        ServicePool pool = new();
        var ex = Assert.Catch<InvalidOperationException>(() => pool.RegisterNow<ITest>());
        Assert.IsNotNull(ex);
        Assert.IsNotEmpty(ex!.Message);
    }

    [TestCase("en_US")]
    [TestCase("es_MX")]
    public void ServicePool_strings_have_localized_messages(string locale)
    {
        Resources.Strings.Errors.Culture = new(locale);
        foreach (var prop in typeof(Resources.Strings.Errors)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => p.GetValue(null)).Cast<string>())
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(prop));
        }
    }

    [Test]
    public void ServicePool_throws_error_on_uninstantiable_class()
    {
        ServicePool pool = new();
        pool.Register(() => 3);
        Assert.Catch<InvalidOperationException>(() => pool.RegisterNow<UninstantiableTest>());
    }

    [Test]
    public void ServicePool_injects_dependencies()
    {
        ServicePool pool = new ServicePool()
            .Register<Random>()
            .Register<Test1>()
            .Register<Test4>();

        var t4 = pool.Resolve<Test4>()!;
        Assert.IsInstanceOf<Test4>(t4);
        Assert.IsInstanceOf<Random>(t4.Random);
        Assert.IsInstanceOf<Test1>(t4.Test1);
    }

    private interface ITest
    {
        [ExcludeFromCodeCoverage]
        void Test();
    }

    [ExcludeFromCodeCoverage]
    private static object DummyFactory()
    {
        return new object();
    }

    [ExcludeFromCodeCoverage]
    private abstract class AbstractTest : ITest
    {
        public abstract void Test();
    }

    [ExcludeFromCodeCoverage]
    private class UninstantiableTest
    {
        public UninstantiableTest(int x, Exception y, Guid z, IEnumerator a)
        {
        }
    }

    [ExcludeFromCodeCoverage]
    private class Test1 : ITest
    {
        public void Test()
        {
            Assert.Pass();
        }
    }

    [ExcludeFromCodeCoverage]
    private class Test2 : ITest
    {
        void ITest.Test()
        {
            Assert.Pass();
        }
    }

    [ExcludeFromCodeCoverage]
    private class Test3 : Test1
    {
    }

    [ExcludeFromCodeCoverage]
    private class Test4
    {
        public Test4(Random random, Test1 test1)
        {
            Random = random;
            Test1 = test1;
        }

        public Random Random { get; }
        public Test1 Test1 { get; }
    }
}
