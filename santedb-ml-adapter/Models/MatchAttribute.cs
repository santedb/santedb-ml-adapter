/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: khannan
 * Date: 2021-9-2
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SanteDB.ML.Adapter.Models
{
	/// <summary>
	/// Represents a match configuration attribute.
	/// </summary>
	public class MatchAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MatchAttribute"/> class.
		/// </summary>
		public MatchAttribute()
		{
			this.Bounds = new List<double>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MatchAttribute"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <exception cref="ArgumentNullException">key - Value cannot be null</exception>
		public MatchAttribute(string key) : this()
		{
			this.Key = string.IsNullOrEmpty(key) ? throw new ArgumentNullException(nameof(key), "Value cannot be null") : key;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MatchAttribute"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="bounds">The bounds.</param>
		public MatchAttribute(string key, List<double> bounds) : this(key)
		{
			this.Bounds = bounds == null || !bounds.Any() ? throw new ArgumentNullException(nameof(bounds), "Value cannot be null") : bounds;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MatchAttribute"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="m">The m.</param>
		/// <param name="u">The u.</param>
		public MatchAttribute(string key, double m, double u) : this(key)
		{
			this.M = m;
			this.U = u;
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
		[JsonPropertyName("key")]
		public string Key { get; set; }

		/// <summary>
		/// Gets or sets the m.
		/// </summary>
		/// <value>The m.</value>
		[JsonPropertyName("m")]
		public double? M { get; set; }

		/// <summary>
		/// Gets or sets the u.
		/// </summary>
		/// <value>The u.</value>
		[JsonPropertyName("u")]
		public double? U { get; set; }

		/// <summary>
		/// Gets or sets the bounds.
		/// </summary>
		/// <value>The bounds.</value>
		[JsonPropertyName("bounds")]
		public List<double> Bounds { get; set; }
	}
}
