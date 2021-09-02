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
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SanteDB.ML.Adapter.Models
{
	/// <summary>
	/// Represents ground truth scores.
	/// </summary>
	public class GroundTruthScores
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroundTruthScores"/> class.
		/// </summary>
		public GroundTruthScores()
		{
			this.Matches = new List<decimal?>();
			this.NonMatches = new List<decimal?>();
		}

		/// <summary>
		/// Gets or sets the matches.
		/// </summary>
		/// <value>The matches.</value>
		[JsonPropertyName("1")]
		public List<decimal?> Matches { get; set; }

		/// <summary>
		/// Gets or sets the non matches.
		/// </summary>
		/// <value>The non matches.</value>
		[JsonPropertyName("0")]
		public List<decimal?> NonMatches { get; set; }
	}
}
