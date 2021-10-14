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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SanteDB.ML.Adapter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;

namespace SanteDB.ML.Adapter.Services.Impl
{
	/// <summary>
	/// Represents a SanteDB match configuration service.
	/// </summary>
	public class SanteMatchConfigurationService : ISanteMatchConfigurationService
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
		private readonly ILogger<SanteMatchConfigurationService> logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="SanteMatchConfigurationService" /> class.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <param name="httpClientFactory">The HTTP client factory.</param>
		/// <param name="authenticationService">The authentication service.</param>
		/// <param name="logger">The logger.</param>
		public SanteMatchConfigurationService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ISanteAuthenticationService authenticationService, ILogger<SanteMatchConfigurationService> logger)
		{
			this.configuration = configuration;
			this.authenticationService = authenticationService;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		/// <summary>
		/// Gets a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <returns>Returns the match configuration.</returns>
		public async Task<MatchConfiguration> GetMatchConfigurationAsync(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException(nameof(id), "Value cannot be null");
			}

			var accessToken = await this.authenticationService.AuthenticateAsync();

			var client = this.httpClientFactory.CreateClient();

			if (client.DefaultRequestHeaders.Accept.All(c => c.MediaType != "application/xml"))
			{
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
			}

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			this.logger.LogDebug($"Attempting to retrieve match config: {id}");

			var response = await client.GetAsync(new Uri($"{this.configuration.GetValue<string>("SanteDBEndpoint")}/ami/MatchConfiguration/{id}"));

			response.EnsureSuccessStatusCode();

			var responseMessage = await response.Content.ReadAsStringAsync();

			var document = new XmlDocument();
			document.LoadXml(responseMessage);

			this.logger.LogDebug($"Match config retrieved: {id}");

			if (document.DocumentElement == null)
			{
				throw new InvalidOperationException($"Match config: {id} does not have a root element");
			}

			var scoringElement = document.DocumentElement.ChildNodes.Cast<XmlElement>().FirstOrDefault(element => element.Name == "scoring");

			if (scoringElement == null)
			{
				throw new InvalidOperationException($"Match config: {id} does not have a scoring section");
			}

			var attributeNodes = scoringElement.ChildNodes.Cast<XmlElement>().Where(c => c.Name == "attribute").Select(c => c.Attributes);

			var matchConfiguration = new MatchConfiguration
			{
				MatchAttributes = attributeNodes.Select(x => new MatchAttribute
				{
					Key = x["property"]?.Value ?? throw new InvalidOperationException("Attribute element does not have a 'property' attribute"),
					M = Convert.ToDouble(x["m"]?.Value ?? throw new InvalidOperationException("Attribute element does not have a 'm' attribute")),
					U = Convert.ToDouble(x["u"]?.Value ?? throw new InvalidOperationException("Attribute element does not have a 'u' attribute")),
					Bounds = new List<double>
					{
						0, 1
					}

				}).ToList()
			};

			matchConfiguration.MatchThreshold = Convert.ToDouble(document.DocumentElement.Attributes["matchThreshold"]?.Value ?? throw new InvalidOperationException("Match configuration element does not have a 'matchThreshold' attribute"));
			matchConfiguration.NonMatchThreshold = Convert.ToDouble(document.DocumentElement.Attributes["nonmatchThreshold"]?.Value ?? throw new InvalidOperationException("Match configuration element does not have a 'nonmatchThreshold' attribute"));

			return matchConfiguration;
		}

		/// <summary>
		/// Updates a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <param name="matchConfiguration">The match configuration.</param>
		/// <returns>Returns the match configuration.</returns>
		public async Task<MatchConfiguration> UpdateMatchConfigurationAsync(string id, MatchConfiguration matchConfiguration)
		{
			await Task.Yield();
			return matchConfiguration;

			// TODO: get config from SanteDB, update match attributes, PUT MatchConfiguration to SanteDB
			//throw new NotImplementedException();
		}
	}
}
