// SugarExtensionsTests.cs
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

using System.Diagnostics.CodeAnalysis;
using TheXDS.ServicePool.Extensions;

namespace TheXDS.ServicePool;

public class SugarExtensionsTests
{
    [Test]
    public void RegisterIf_T_skips_if_false()
    {
        Pool pool = new();
        pool.RegisterIf<Random>(false);
        Assert.That(pool.Count, Is.Zero);
    }

    [Test]
    public void RegisterIf_T_registers_if_true()
    {
        Pool pool = new();
        pool.RegisterIf<Random>(true);
        Assert.That(pool.Count, Is.EqualTo(1));
    }

    [Test]
    public void RegisterIf_with_factory_skips_if_false()
    {
        Pool pool = new();
        pool.RegisterIf(false, DummyFactory);
        Assert.That(pool.Count, Is.Zero);
    }

    [Test]
    public void RegisterIf_with_factory_registers_if_true()
    {
        Pool pool = new();
        pool.RegisterIf(true, DummyFactory);
        Assert.That(pool.Count, Is.EqualTo(1));
    }

    [Test]
    public void RegisterNowIf_T_skips_if_false()
    {
        Pool pool = new();
        pool.RegisterNowIf<Random>(false);
        Assert.That(pool.Count, Is.Zero);
    }

    [Test]
    public void RegisterNowIf_T_registers_if_true()
    {
        Pool pool = new();
        pool.RegisterNowIf<Random>(true);
        Assert.That(pool.Count, Is.EqualTo(1));
    }

    [Test]
    public void RegisterNowIf_with_singleton_skips_if_false()
    {
        Pool pool = new();
        pool.RegisterNowIf(false, new Exception());
        Assert.That(pool.Count, Is.Zero);
    }

    [Test]
    public void RegisterNowIf_with_singleton_registers_if_true()
    {
        Pool pool = new();
        pool.RegisterNowIf(true, new Exception());
        Assert.That(pool.Count, Is.EqualTo(1));
    }

    [ExcludeFromCodeCoverage]
    private static object DummyFactory()
    {
        return new object();
    }
}
