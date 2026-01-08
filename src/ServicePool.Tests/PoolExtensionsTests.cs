// PoolExtensionsTests.cs
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

using TheXDS.ServicePool.Extensions;

namespace TheXDS.ServicePool;

public class PoolExtensionsTests
{
    [Test]
    public void RegisterInto_registers_singleton()
    {
        var pool = new Pool();
        var x = new Random().RegisterInto(pool);
        Assert.That(x, Is.InstanceOf<Random>());
        Assert.That(pool.Resolve<Random>(), Is.SameAs(x));
    }

    [Test]
    public void RegisterIntoIf_registers_if_true()
    {
        var pool = new Pool();
        var x = new Random().RegisterIntoIf(pool, true);
        Assert.That(x, Is.InstanceOf<Random>());
        Assert.That(pool.Resolve<Random>(), Is.SameAs(x));
    }

    [Test]
    public void RegisterIntoIf_returns_object_if_false()
    {
        var pool = new Pool();
        var x = new Random().RegisterIntoIf(pool, false);
        Assert.That(x, Is.InstanceOf<Random>());
        Assert.That(pool.Resolve<Random>(), Is.Null);
    }
}
