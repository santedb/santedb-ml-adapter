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

using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SanteDB.ML.Adapter.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter.Services.Impl
{
	/// <summary>
	/// Represents a SanteDB ground truth service.
	/// </summary>
	public class SanteGroundTruthService : ISanteGroundTruthService
	{
		/// <summary>
		/// The authentication service.
		/// </summary>
		private readonly ISanteAuthenticationService authenticationService;

		/// <summary>
		/// The configuration.
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// The HTTP client factory.
		/// </summary>
		private readonly IHttpClientFactory httpClientFactory;

		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<SanteGroundTruthService> logger;

		/// <summary>
		/// The FHIR map service.
		/// </summary>
		private readonly ISanteFhirMapService fhirMapService;

		/// <summary>
		/// Initializes a new instance of the <see cref="SanteGroundTruthService" /> class.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <param name="httpClientFactory">The HTTP client factory.</param>
		/// <param name="authenticationService">The authentication service.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="fhirMapService">The FHIR map service.</param>
		public SanteGroundTruthService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ISanteAuthenticationService authenticationService, ILogger<SanteGroundTruthService> logger, ISanteFhirMapService fhirMapService)
		{
			this.authenticationService = authenticationService;
			this.configuration = configuration;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
			this.fhirMapService = fhirMapService;

		}

		/// <summary>
		/// Gets ground truth scores asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <returns>Returns the ground truth scores.</returns>
		public async Task<GroundTruthScores> GetGroundTruthScoresAsync(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException(nameof(id), "Value cannot be null");
			}

			var accessToken = await this.authenticationService.AuthenticateAsync();

			var client = this.httpClientFactory.CreateClient();

			if (client.DefaultRequestHeaders.Accept.All(c => c.MediaType != "application/fhir+xml"))
			{
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/fhir+xml"));
			}

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			this.logger.LogDebug($"Attempting to retrieve match config: {id}");

			var response = await client.GetAsync(new Uri($"{this.configuration.GetValue<string>("SanteDBEndpoint")}/fhir/Patient/$mdm-query-links?_configurationName={id}"));

			response.EnsureSuccessStatusCode();

			var responseMessage = await response.Content.ReadAsStringAsync();

			var parser = new FhirXmlParser(ParserSettings.CreateDefault());

			var parameters = parser.Parse<Parameters>(responseMessage);

			return this.fhirMapService.MapGroundTruthScores(parameters);
		}
	}
}
