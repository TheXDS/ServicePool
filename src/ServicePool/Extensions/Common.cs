// Common.cs
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

namespace TheXDS.ServicePool.Extensions
{
    /// <summary>
    /// Includes a set of useful extension methods to work with ServicePool.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Adds support for Fluent singleton registration on any object type.
        /// </summary>
        /// <typeparam name="T">
        /// Type of object to register into the pool.
        /// </typeparam>
        /// <param name="obj">Instance to register.</param>
        /// <param name="pool">Pool to register the singleton into.</param>
        /// <returns>The same instance as <paramref name="obj"/>.</returns>
        public static T RegisterInto<T>(this T obj, ServicePool pool) where T : notnull
        {
            pool.RegisterNow(obj);
            return obj;
        }

        /// <summary>
        /// Adds support for Fluent singleton registration on any object type.
        /// </summary>
        /// <typeparam name="T">
        /// Type of object to register into the pool.
        /// </typeparam>
        /// <param name="obj">Instance to register.</param>
        /// <param name="pool">Pool to register the singleton into.</param>
        /// <param name="condition">
        /// Value that determines if the singleton should be registered or not.
        /// </param>
        /// <returns>
        /// The same instance as <paramref name="obj"/> if 
        /// <paramref name="condition"/> is equal to <see langword="true"/>,
        /// <see langword="null"/> otherwise.</returns>
        public static T? RegisterIntoIf<T>(this T obj, ServicePool pool, bool condition) where T : notnull
        {
            return condition ? obj.RegisterInto(pool) : default;
        }
    }
}
