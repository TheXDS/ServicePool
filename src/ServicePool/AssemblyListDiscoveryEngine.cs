// AssemblyListDiscoveryEngine.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace TheXDS.ServicePool
{
    /// <summary>
    /// Implements a <see cref="IDiscoveryEngine"/> that searches for types in
    /// an internal list of assemblies that have been previously registered.
    /// </summary>
    public class AssemblyListDiscoveryEngine : Collection<Assembly>, IDiscoveryEngine
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AssemblyListDiscoveryEngine"/> class.
        /// </summary>
        public AssemblyListDiscoveryEngine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AssemblyListDiscoveryEngine"/> class.
        /// </summary>
        /// <param name="list">
        /// The list that is wrapped by the new collection.
        /// </param>
        public AssemblyListDiscoveryEngine(IList<Assembly> list) : base(list)
        {
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, Assembly item)
        {
            base.InsertItem(index, item ?? throw new ArgumentNullException(nameof(item)));
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, Assembly item)
        {
            base.SetItem(index, item ?? throw new ArgumentNullException(nameof(item)));
        }

        /// <inheritdoc/>
        public IEnumerable<Type> Discover(Type t)
        {
            return this.SelectMany(p => p.GetTypes())
                .Where(p => !p.IsAbstract && !p.IsInterface && t.IsAssignableFrom(p));
        }
    }
}
