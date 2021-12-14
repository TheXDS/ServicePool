// DefaultDiscoveryEngine.cs
//
// This file is part of ServicePool
//
// Author(s):
//      César Andrés Morgan <xds_xps_ivx@hotmail.com>
//
// Copyright (C) 2021 César Andrés Morgan
//
// ServicePool is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later
// version.
//
// ServicePool is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY, without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TheXDS.ServicePool
{
    /// <summary>
    /// Implements a <see cref="IDiscoveryEngine"/> that searches for types in 
    /// the current <see cref="AppDomain"/>.
    /// </summary>
    /// <remarks>
    /// This <see cref="IDiscoveryEngine"/> is used by default by
    /// <see cref="ServicePool.Discover{T}(bool)"/> and
    /// <see cref="ServicePool.DiscoverAll{T}(bool)"/>.
    /// </remarks>
    public class DefaultDiscoveryEngine : IDiscoveryEngine
    {
        /// <inheritdoc/>
        public IEnumerable<Type> Discover(Type t)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(p => p.GetTypes())
                .Where(p => !p.IsAbstract && !p.IsInterface && t.IsAssignableFrom(p));
        }
    }
}
