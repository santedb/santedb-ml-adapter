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

using SanteDB.ML.Adapter.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter.Services
{
	/// <summary>
	/// Represents a SanteDB match configuration service.
	/// </summary>
	public interface ISanteMatchConfigurationService
	{
		/// <summary>
		/// Gets a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <returns>Returns the match configuration.</returns>
		public Task<MatchConfiguration> GetMatchConfigurationAsync(string id);

		/// <summary>
		/// Updates a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <param name="matchConfiguration">The match configuration.</param>
		/// <returns>Returns the match configuration.</returns>
		public Task<MatchConfiguration> UpdateMatchConfigurationAsync(string id, MatchConfiguration matchConfiguration);
	}
}
