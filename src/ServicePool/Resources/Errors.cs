// Errors.cs
//
// This file is part of ServicePool
//
// Author(s):
//      César Andrés Morgan <xds_xps_ivx@hotmail.com>
//
// Released under the MIT License (MIT)
// Copyright © 2021 - 2023 César Andrés Morgan
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
using TheXDS.ServicePool.Exceptions;

namespace TheXDS.ServicePool.Resources;

/// <summary>
/// Exposes a set of common errors that can be thrown by ServicePool.
/// </summary>
public static class Errors
{
    /// <summary>
    /// Gets a new instance of a <see cref="ServiceAlreadyRegisteredException"/>
    /// that is normally thrown when the user tries to register a service of a
    /// type that has been registered already on a <see cref="Pool"/>.
    /// </summary>
    /// <returns>
    /// A new instance of the <see cref="ServiceAlreadyRegisteredException"/>
    /// class.
    /// </returns>
    public static ServiceAlreadyRegisteredException ServiceAlreadyRegistered() => new();

    /// <summary>
    /// Gets a new instance of the <see cref="TypeNotInstantiableException"/>
    /// that is normally thrown when a type is not instantiable; that is, when
    /// the type is abstract, static or it does not have any public constructors.
    /// </summary>
    /// <param name="t">
    /// Offending type that is the cause of the exception.
    /// </param>
    /// <returns>
    /// A new instance of the <see cref="TypeNotInstantiableException"/> class.
    /// </returns>
    public static TypeNotInstantiableException TypeNotInstantiable(Type t) => new(t);

    /// <summary>
    /// Gets a new instance of the <see cref="MissingDependencyException"/>
    /// that is normally thrown when trying to instantiate a service type and
    /// not being able to find a suitable collection of dependencies to inject
    /// while searching for a constructor.
    /// </summary>
    /// <param name="t">Array of dependency types that were requested.</param>
    /// <returns>
    /// A new instance of the <see cref="MissingDependencyException"/> class.
    /// </returns>
    public static MissingDependencyException MissingDependency(Type[][] t) => new(t);
}
