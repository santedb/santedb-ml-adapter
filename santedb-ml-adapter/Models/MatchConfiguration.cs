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
 * Date: 2021-10-14
 */

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SanteDB.ML.Adapter.Models
{
	/// <summary>
	/// Represents a match configuration.
	/// </summary>
	public class MatchConfiguration
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MatchConfiguration"/> class.
		/// </summary>
		public MatchConfiguration()
		{
			
		}

		/// <summary>
		/// Gets or sets the match threshold.
		/// </summary>
		/// <value>The match threshold.</value>
		[JsonPropertyName("matchThreshold")]
		public double MatchThreshold { get; set; }

		/// <summary>
		/// Gets or sets the non match threshold.
		/// </summary>
		/// <value>The non match threshold.</value>
		[JsonPropertyName("nonMatchThreshold")]
		public double NonMatchThreshold { get; set; }

		/// <summary>
		/// Gets or sets the match attributes.
		/// </summary>
		/// <value>The match attributes.</value>
		[JsonPropertyName("attributes")]
		public List<MatchAttribute> MatchAttributes { get; set; }
	}
}
