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

using Microsoft.Extensions.Logging;
using SanteDB.ML.Adapter.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter.Services.Impl
{
	/// <summary>
	/// Represents a SanteDB match configuration service.
	/// </summary>
	public class SanteMatchConfigurationService : ISanteMatchConfigurationService
	{
		/// <summary>
		/// The HTTP client factory.
		/// </summary>
		private readonly IHttpClientFactory httpClientFactory;

		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<SanteMatchConfigurationService> logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="SanteMatchConfigurationService"/> class.
		/// </summary>
		/// <param name="httpClientFactory">The HTTP client factory.</param>
		/// <param name="logger">The logger.</param>
		public SanteMatchConfigurationService(IHttpClientFactory httpClientFactory, ILogger<SanteMatchConfigurationService> logger)
		{
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		/// <summary>
		/// Gets a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <returns>Returns the match configuration.</returns>
		public async Task<List<MatchAttribute>> GetMatchConfigurationAsync(string id)
		{
			await Task.Yield();

			var matchAttributes = new List<MatchAttribute>();

			matchAttributes.Add(new MatchAttribute("relationship[Mother].target.name", 0.77, 0.3));
			matchAttributes.Add(new MatchAttribute("dateOfBirth", 0.5, 0.5));
			matchAttributes.Add(new MatchAttribute("identifier[SSN].value", 0.8, 0.1));

			return matchAttributes;
		}

		/// <summary>
		/// Maps a match configuration.
		/// </summary>
		/// <param name="matchConfiguration">The match configuration.</param>
		/// <returns>Returns the list of mapped match attributes.</returns>
		private List<MatchAttribute> MapMatchConfiguration(object matchConfiguration)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Updates a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <param name="matchAttributes">The match attributes.</param>
		/// <returns>Returns the match configuration.</returns>
		public Task<List<MatchAttribute>> UpdateMatchConfigurationAsync(string id, List<MatchAttribute> matchAttributes)
		{
			throw new NotImplementedException();
		}
	}
}
