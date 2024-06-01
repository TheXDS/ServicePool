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
/// Exception thrown when the user tries to register a service of a type that
/// has been registered already on a <see cref="Pool"/>.
/// </summary>
public class ServiceAlreadyRegisteredException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ServiceAlreadyRegisteredException"/> class.
    /// </summary>
    public ServiceAlreadyRegisteredException() : base(Ers.ServiceAlreadyRegistered)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ServiceAlreadyRegisteredException"/> class.
    /// </summary>
    /// <param name="message">Message that describes the exception.</param>
    public ServiceAlreadyRegisteredException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ServiceAlreadyRegisteredException"/> class.
    /// </summary>
    /// <param name="message">Message that describes the exception.</param>
    /// <param name="innerException">
    /// <see cref="Exception"/> that is the cause of this exception.
    /// </param>
    public ServiceAlreadyRegisteredException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
