﻿/*
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

using Hl7.Fhir.Model;
using SanteDB.ML.Adapter.Models;
using System.Linq;

namespace SanteDB.ML.Adapter.Services.Impl
{
	/// <summary>
	/// Represents a SanteDB FHIR map service.
	/// </summary>
	public class SanteFhirMapService : ISanteFhirMapService
	{
		/// <summary>
		/// The link key.
		/// </summary>
		private const string LinkKey = "link";

		/// <summary>
		/// The match result key.
		/// </summary>
		private const string MatchResultKey = "matchResult";

		/// <summary>
		/// The match key.
		/// </summary>
		private const string MatchKey = "MATCH";

		/// <summary>
		/// The non match key.
		/// </summary>
		private const string NonMatchKey = "NO_MATCH";

		/// <summary>
		/// The score key
		/// </summary>
		private const string ScoreKey = "score";

		/// <summary>
		/// Maps ground truth scores.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		/// <returns>Returns the mapped ground truth scores.</returns>
		public GroundTruthScores MapGroundTruthScores(Parameters parameters)
		{
			var groundTruthScores = new GroundTruthScores();

			groundTruthScores.Matches.AddRange(parameters.Parameter.Where(c => c.Name == LinkKey)
				.Where(c => c.Part.Any(x => x.Name == MatchResultKey && ((FhirString)x.Value).Value == MatchKey))
				.Where(c => c.Part.Any(x => x.Name == ScoreKey))
				.SelectMany(c => c.Part.Where(x => x.Name == ScoreKey))
				.Select(c => ((FhirDecimal)c.Value).Value));

			groundTruthScores.NonMatches.AddRange(parameters.Parameter.Where(c => c.Name == LinkKey)
				.Where(c => c.Part.Any(x => x.Name == MatchResultKey && ((FhirString)x.Value).Value == NonMatchKey))
				.Where(c => c.Part.Any(x => x.Name == ScoreKey))
				.SelectMany(c => c.Part.Where(x => x.Name == ScoreKey))
				.Select(c => ((FhirDecimal)c.Value).Value));

			return groundTruthScores;
		}
	}
}
