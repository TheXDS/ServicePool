// ServiceAlreadyRegisteredException.cs
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

using System;
using Ers = TheXDS.ServicePool.Resources.Strings.Errors;

namespace TheXDS.ServicePool.Exceptions;

/// <summary>
/// Exception thrown when a <see cref="Pool"/> is unable to resolve the
/// required dependencies to use any of the available constructors for a type
/// that needs to be instantiated.
/// </summary>
public class MissingDependencyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="MissingDependencyException"/> class.
    /// </summary>
    /// <param name="missingDependencies">
    /// Array of dependency arrays that were tried for resolution.
    /// </param>
    public MissingDependencyException(Type[][] missingDependencies): this(missingDependencies, Ers.MissingDependencies)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="MissingDependencyException"/> class.
    /// </summary>
    /// <param name="missingDependencies">
    /// Array of dependency arrays that were tried for resolution.
    /// </param>
    /// <param name="message">Message that describes the exception.</param>
    public MissingDependencyException(Type[][] missingDependencies, string? message) : base(message)
    {
        MissingDependencies = missingDependencies;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="MissingDependencyException"/> class.
    /// </summary>
    /// <param name="missingDependencies">
    /// Array of dependency arrays that were tried for resolution.
    /// </param>
    /// <param name="message">Message that describes the exception.</param>
    /// <param name="innerException">
    /// <see cref="Exception"/> that is the cause of this exception.
    /// </param>
    public MissingDependencyException(Type[][] missingDependencies, string? message, Exception? innerException) : base(message, innerException)
    {
        MissingDependencies = missingDependencies;
    }

    /// <summary>
    /// Gets an array of the dependencies that were tried for dependency resolution.
    /// </summary>
    public Type[][] MissingDependencies { get; }
}
