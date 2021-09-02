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
using System.Net.Http;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter.Services.Impl
{
	/// <summary>
	/// Represents a SanteDB ground truth service.
	/// </summary>
	public class SanteGroundTruthService : ISanteGroundTruthService
	{
		/// <summary>
		/// The HTTP client factory.
		/// </summary>
		private readonly IHttpClientFactory httpClientFactory;

		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<SanteGroundTruthService> logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="SanteGroundTruthService"/> class.
		/// </summary>
		/// <param name="httpClientFactory">The HTTP client factory.</param>
		/// <param name="logger">The logger.</param>
		public SanteGroundTruthService(IHttpClientFactory httpClientFactory, ILogger<SanteGroundTruthService> logger)
		{
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		/// <summary>
		/// Gets ground truth scores asynchronously.
		/// </summary>
		/// <param name="matchConfigurationId">The id of the match configuration.</param>
		/// <returns>Returns the ground truth scores.</returns>
		public Task<GroundTruthScores> GetGroundTruthScoresAsync(string matchConfigurationId)
		{
			throw new NotImplementedException();
		}
	}
}
