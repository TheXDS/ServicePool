// PoolTests.cs
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

using System.Collections;
using TheXDS.ServicePool.Exceptions;
using TheXDS.ServicePool.TestTypes;

namespace TheXDS.ServicePool;

public class PoolTests
{
    [Test]
    public void Count_gets_total_service_count()
    {
        Pool pool = new();
        Assert.That(pool.Count, Is.Zero);

        pool.RegisterNow<Exception>();
        Assert.That(pool.Count, Is.EqualTo(1));

        pool.RegisterNow<EventArgs>();
        Assert.That(pool.Count, Is.EqualTo(2));

        pool.Register<Random>();
        Assert.That(pool.Count, Is.EqualTo(3));
    }

    [Test]
    public void Pool_throws_on_registration_already_existing()
    {
        Pool pool = new();
        pool.Register<Random>();
        Assert.That(() => pool.Register<Random>(), Throws.InstanceOf<ServiceAlreadyRegisteredException>());
    }

    [Test]
    public void Pool_registers_and_resolves_cached_services()
    {
        Pool pool = new();
        pool.RegisterNow<Random>();
        Assert.That(pool.Resolve<Random>(), Is.InstanceOf<Random>());
    }

    [Test]
    public void Pool_resolves_persistent_services()
    {
        Pool pool = new();
        pool.Register<Random>(true);
        Random? r1 = pool.Resolve<Random>();
        Random? r2 = pool.Resolve<Random>();

        Assert.That(r1, Is.InstanceOf<Random>());
        Assert.That(r2, Is.InstanceOf<Random>());
        Assert.That(r1, Is.SameAs(r2));
    }

    [Test]
    public void InitNow_initializes_services()
    {
        Pool pool = new();
        pool.Register<Random>(true);
        pool.InitNow();
        Assert.That(pool.Count, Is.Not.Zero);
    }

    [Test]
    public void Pool_resolves_non_persistent_services()
    {
        Pool pool = new();
        pool.Register<Random>(false);
        Random? r1 = pool.Resolve<Random>();
        Random? r2 = pool.Resolve<Random>();

        Assert.That(r1, Is.InstanceOf<Random>());
        Assert.That(r2, Is.InstanceOf<Random>());
        Assert.That(r1, Is.Not.SameAs(r2));
    }

    [Test]
    public void Resolve_returns_null_if_not_registered()
    {
        Pool pool = new();
        Assert.That(pool.Resolve<Random>(), Is.Null);
    }

    [Test]
    public void Pool_supports_removal_of_lazy_services()
    {
        Pool pool = new();
        pool.Register<Test1>();

        Assert.That(pool.Remove<Test1>(), Is.True);
        Assert.That(pool.Count, Is.Zero);
    }

    [Test]
    public void Pool_supports_removal_of_eager_services()
    {
        Pool pool = new();
        pool.RegisterNow<Test1>();

        Assert.That(pool.Remove<Test1>(), Is.True);
        Assert.That(pool.Count, Is.Zero);
    }

    [Test]
    public void Remove_returns_false_if_service_is_not_registered()
    {
        Pool pool = new();
        Assert.That(pool.Remove<Test2>(), Is.False);
        Assert.That(pool.Count, Is.Zero);
    }

    [Test]
    public void Consume_removes_service()
    {
        Pool pool = new();
        pool.RegisterNow<Test1>();
        Assert.That(pool.Consume<Test1>(), Is.InstanceOf<Test1>());
        Assert.That(pool.Count, Is.Zero);
        Assert.That(pool.Consume<Test1>(), Is.Null);
    }

    [Test]
    public void Consume_returns_null_if_no_service_is_available()
    {
        Pool pool = new();
        Assert.That(pool.Consume<Test1>(), Is.Null);
    }

    [Test]
    public void Consume_returns_null_if_service_was_consumed()
    {
        Pool pool = new();
        pool.RegisterNow<Test1>();
        pool.Consume<Test1>();

        Assert.That(pool.Consume<Test1>(), Is.Null);
    }

    [Test]
    public void Enumerator_includes_lazy_and_eager_items()
    {
        Pool pool = new();
        pool.RegisterNow<Test1>();
        pool.Register<Test2>();

        IEnumerator e = pool.GetEnumerator();
        Assert.That(e, Is.InstanceOf<IEnumerator>());
        int c = 0;
        while (e.MoveNext())
        {
            Assert.That(e.Current, Is.InstanceOf<ITest>());
            c++;
        }
        Assert.That(c, Is.EqualTo(2));
    }

    [Test]
    public void ServicePool_throws_error_on_invalid_registrations()
    {
        Pool pool = new();
        Assert.Catch<TypeNotInstantiableException>(pool.RegisterNow<ITest>);
        Assert.Catch<TypeNotInstantiableException>(pool.RegisterNow<AbstractTest>);
    }

    [Test]
    public void ServicePool_errors_have_messages()
    {
        Pool pool = new();
        var ex = Assert.Catch<TypeNotInstantiableException>(pool.RegisterNow<ITest>);
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Is.Not.Empty);
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
            Assert.That(string.IsNullOrWhiteSpace(prop), Is.False);
        }
    }

    [Test]
    public void ServicePool_throws_error_on_uninstantiable_class()
    {
        Pool pool = new();
        pool.Register(() => 3);
        Assert.Catch<MissingDependencyException>(pool.RegisterNow<UninstantiableTest>);
    }

    [Test]
    public void ServicePool_injects_dependencies()
    {
        Pool pool = new();
        pool.Register<Random>();
        pool.Register<Test1>();
        pool.Register<Test4>();

        var t4 = pool.Resolve<Test4>()!;
        Assert.That(t4, Is.InstanceOf<Test4>());
        Assert.That(t4.Random, Is.InstanceOf<Random>());
        Assert.That(t4.Test1, Is.InstanceOf<Test1>());
    }
    [Test]
    public void Pool_config_supports_flex_registration()
    {
        var c = PoolConfig.FlexResolve;
        var pool = new Pool(c);
        pool.RegisterNow(new Test1());
        Assert.That(pool.Resolve<ITest>(), Is.Not.Null);
    }

    [Test]
    public void Pool_config_flex_registration_defaults_to_false()
    {
        var c = PoolConfig.Default;
        var pool = new Pool(c);
        pool.RegisterNow(new Test1());
        Assert.That(pool.Resolve<ITest>(), Is.Null);
    }

    [Test]
    public void Resolve_resolves_for_interfaces()
    {
        Pool? pool = new();
        pool.Register<ITest, Test1>();
        Assert.That(pool.Resolve<ITest>(), Is.Not.Null);
    }

    [Test]
    public void Resolve_resolves_for_method_registration()
    {
        Pool? pool = new();
        pool.Register(() => new Test1());
        Assert.That(pool.Resolve<Test1>(), Is.Not.Null);
    }

    [Test]
    public void Resolve_resolves_for_base_type_for_method_registration()
    {
        Pool? pool = new();
        pool.Register<ITest>(() => new Test1());
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
