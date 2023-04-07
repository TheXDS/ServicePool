// FilterableDiscoveryEngineTests.cs
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
using System.Linq;

namespace TheXDS.ServicePool.Tests;

public class FilterableDiscoveryEngineTests
{
    [Test]
    public void Engine_filters_out_unwanted_types_with_params_ctor()
    {
        var e = new FilterableDiscoveryEngine(typeof(InvalidOperationException), typeof(StackOverflowException));
        Assert.That(e.Discover(typeof(Exception)), Does.Not.Contain(typeof(InvalidOperationException)));
        Assert.That(e.Discover(typeof(Exception)), Does.Not.Contain(typeof(StackOverflowException)));
        Assert.That(e.Discover(typeof(Exception)), Contains.Item(typeof(ArgumentException)));
        Assert.That(e.Discover(typeof(Exception)), Contains.Item(typeof(NullReferenceException)));
    }

    [Test]
    public void Engine_filters_out_unwanted_types_with_enumerable_ctor()
    {
        var e = new FilterableDiscoveryEngine(new[] { typeof(InvalidOperationException), typeof(StackOverflowException) }.AsEnumerable());
        Assert.That(e.Discover(typeof(Exception)), Does.Not.Contain(typeof(InvalidOperationException)));
        Assert.That(e.Discover(typeof(Exception)), Does.Not.Contain(typeof(StackOverflowException)));
        Assert.That(e.Discover(typeof(Exception)), Contains.Item(typeof(ArgumentException)));
        Assert.That(e.Discover(typeof(Exception)), Contains.Item(typeof(NullReferenceException)));
    }

    [Test]
    public void Engine_filters_out_unwanted_types_with_exclusion_registration()
    {
        var e = new FilterableDiscoveryEngine()
        {
            Exclusions =
            {
                typeof(InvalidOperationException),
                typeof(StackOverflowException),
            }
        };
        Assert.That(e.Discover(typeof(Exception)), Does.Not.Contain(typeof(InvalidOperationException)));
        Assert.That(e.Discover(typeof(Exception)), Does.Not.Contain(typeof(StackOverflowException)));
        Assert.That(e.Discover(typeof(Exception)), Contains.Item(typeof(ArgumentException)));
        Assert.That(e.Discover(typeof(Exception)), Contains.Item(typeof(NullReferenceException)));
    }

    [Test]
    public void Engine_contains_exclusions()
    {
        var e = new FilterableDiscoveryEngine()
        {
            Exclusions =
            {
                typeof(InvalidOperationException),
                typeof(StackOverflowException),
            }
        };
        Assert.That(e.Exclusions, Contains.Item(typeof(InvalidOperationException)));
        Assert.That(e.Exclusions, Contains.Item(typeof(StackOverflowException)));
        Assert.That(e.Exclusions, Does.Not.Contain(typeof(ArgumentException)));
        Assert.That(e.Exclusions, Does.Not.Contain(typeof(NullReferenceException)));
    }
}
