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
using System.Globalization;
using System.IO;
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

			var document = await this.GetMatchConfigurationRawAsync(id);

			return MapToMatchConfiguration(document);
		}

		/// <summary>
		/// Gets a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <returns>Returns the match configuration.</returns>
		private async Task<XmlDocument> GetMatchConfigurationRawAsync(string id)
		{
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

			return document;
		}

		/// <summary>
		/// Maps to match configuration.
		/// </summary>
		/// <param name="document">The document.</param>
		/// <returns>Returns the mapped match configuration.</returns>
		/// <exception cref="InvalidOperationException">Match config does not have a root element</exception>
		/// <exception cref="InvalidOperationException">Match config does not have a scoring section</exception>
		/// <exception cref="InvalidOperationException">Match configuration element does not have a 'matchThreshold' attribute</exception>
		/// <exception cref="InvalidOperationException">Match configuration element does not have a 'nonmatchThreshold' attribute</exception>
		private static MatchConfiguration MapToMatchConfiguration(XmlDocument document)
		{
			if (document.DocumentElement == null)
			{
				throw new InvalidOperationException("Match config does not have a root element");
			}

			var scoringElement = document.DocumentElement.ChildNodes.Cast<XmlElement>().FirstOrDefault(element => element.Name == "scoring");

			if (scoringElement == null)
			{
				throw new InvalidOperationException("Match config does not have a scoring section");
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
						1e-8, 1 - 1e-8
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
			var remoteConfiguration = await this.GetMatchConfigurationRawAsync(id);

			if (remoteConfiguration.DocumentElement == null)
			{
				throw new InvalidOperationException("Match config does not have a root element");
			}

			// update the match and nonmatch thresholds
			remoteConfiguration.DocumentElement.Attributes["matchThreshold"].Value = matchConfiguration.MatchThreshold.ToString(CultureInfo.InvariantCulture);
			remoteConfiguration.DocumentElement.Attributes["nonmatchThreshold"].Value = matchConfiguration.NonMatchThreshold.ToString(CultureInfo.InvariantCulture);

			var scoringElement = remoteConfiguration.DocumentElement.ChildNodes.Cast<XmlElement>().FirstOrDefault(element => element.Name == "scoring");

			if (scoringElement == null)
			{
				throw new InvalidOperationException("Match config does not have a scoring section");
			}

			var attributeNodes = scoringElement.ChildNodes.Cast<XmlElement>().Where(c => c.Name == "attribute").Select(c => c.Attributes);

			// find each attribute based on property name and update the M and U values accordingly
			matchConfiguration.MatchAttributes.ForEach(matchAttribute =>
			{
				// HACK
				attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key)["m"].Value = matchAttribute.M.ToString();
				attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key)["u"].Value = matchAttribute.U.ToString();

				// (this.m_m.Value / this.m_u.Value).Ln() / (2.0d).Ln()
				if (attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key)["matchWeight"] != null)
				{
					attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key).Append(remoteConfiguration.CreateAttribute("matchWeight")).Value = Convert.ToString((matchAttribute.M / matchAttribute.U).Ln() / (2.0d).Ln(), CultureInfo.InvariantCulture);
				}
				else
				{
					attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key)["matchWeight"].Value = Convert.ToString((matchAttribute.M / matchAttribute.U).Ln() / (2.0d).Ln(), CultureInfo.InvariantCulture);
				}

				// HACK
				if (attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key)["nonMatchWeight"] != null)
				{
					attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key).Append(remoteConfiguration.CreateAttribute("nonMatchWeight")).Value = Convert.ToString(((1 - matchAttribute.M) / (1 - matchAttribute.U)).Ln() / (2.0d).Ln(), CultureInfo.InvariantCulture);
				}
				else
				{
					attributeNodes.FirstOrDefault(c => c["property"].Value == matchAttribute.Key)["nonMatchWeight"].Value = Convert.ToString(((1 - matchAttribute.M) / (1 - matchAttribute.U)).Ln() / (2.0d).Ln(), CultureInfo.InvariantCulture);
				}
			});

			// PUT the updated config to SanteDB

			var accessToken = await this.authenticationService.AuthenticateAsync();

			var client = this.httpClientFactory.CreateClient();

			if (client.DefaultRequestHeaders.Accept.All(c => c.MediaType != "application/xml"))
			{
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
			}

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			var memoryStream = new MemoryStream();

			remoteConfiguration.Save(memoryStream);

			await memoryStream.FlushAsync();
			memoryStream.Position = 0;

			var content = new StreamContent(memoryStream);

			content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

			var response = await client.PutAsync(new Uri($"{this.configuration.GetValue<string>("SanteDBEndpoint")}/ami/MatchConfiguration/{id}"), content);

			response.EnsureSuccessStatusCode();

			// re-retrieve and return the most recent copy of the match configuration from SanteDB
			return await this.GetMatchConfigurationAsync(id);
		}
	}

	/// <summary>
	/// Represents mathematical extensions for natural logarithms.
	/// </summary>
	public static class MathExtensions
	{
		/// <summary>
		/// The LN function (1/log(source)).
		/// </summary>
		/// <param name="source">The source value.</param>
		/// <returns>Returns the calculated value.</returns>
		public static double Ln(this double source)
		{
			return Math.Log(source, Math.E);
		}


		/// <summary>
		/// The LN function (1/log(source))
		/// </summary>
		/// <param name="source">The source value.</param>
		/// <returns>Returns the calculated value.</returns>
		public static double Ln(this double? source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source), "Value cannot be null");
			}

			return Math.Log(source.Value, Math.E);
		}
	}
}
