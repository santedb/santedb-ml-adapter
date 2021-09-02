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
 * Date: 2021-8-11
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SanteDB.ML.Adapter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter.Controllers
{
	/// <summary>
	/// Represents a configuration controller.
	/// </summary>
	[ApiController]
	[Route("config")]
	[Authorize(AuthenticationSchemes = "Basic")]
	public class ConfigurationController : ControllerBase
	{

		/// <summary>
		/// The default content type.
		/// </summary>
		private const string DefaultContentType = "application/json";

		/// <summary>
		/// The serializer options.
		/// </summary>
		private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
		{
			AllowTrailingCommas = false,
			IgnoreReadOnlyProperties = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			IgnoreNullValues = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true
		};

		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<ConfigurationController> logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationController"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public ConfigurationController(ILogger<ConfigurationController> logger)
		{
			this.logger = logger;
		}

		/// <summary>
		/// Retrieves a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <returns>Returns the match configuration.</returns>
		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetAsync(string id)
		{
			IActionResult result = this.BadRequest();

			if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
			{
				this.logger.LogError("The id value cannot be null or empty");
				return result;
			}

			try
			{
				await Task.Yield();

				var matchAttributes = new List<MatchAttribute>();

				matchAttributes.Add(new MatchAttribute("relationship[Mother].target.name", 0.77, 0.3));
				matchAttributes.Add(new MatchAttribute("dateOfBirth", 0.5, 0.5));
				matchAttributes.Add(new MatchAttribute("identifier[SSN].value",0.8, 0.1));

				result = new ContentResult
				{
					Content = JsonSerializer.Serialize(matchAttributes, serializerOptions),
					ContentType = DefaultContentType,
					StatusCode = (int)HttpStatusCode.OK
				};
			}
			catch (Exception e)
			{
				this.logger.LogError($"Unable to retrieve configuration: {e}");
				result = new ContentResult
				{
					StatusCode = (int)HttpStatusCode.InternalServerError
				};
			}

			return result;
		}

		/// <summary>
		/// Retrieves a match configuration specification asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration.</param>
		/// <returns>Returns the match configuration specification.</returns>
		[HttpGet]
		[Route("{id}/spec")]
		public async Task<IActionResult> GetConfigurationSpecificationAsync(string id)
		{
			IActionResult result = this.BadRequest();

			if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
			{
				this.logger.LogError("The id value cannot be null or empty");
				return result;
			}

			try
			{
				await Task.Yield();

				var matchAttributes = new List<MatchAttribute>();

				matchAttributes.Add(new MatchAttribute("relationship[Mother].target.name", new List<double>
				{
					0, 1
				}));

				matchAttributes.Add(new MatchAttribute("dateOfBirth", new List<double>
				{
					0, 1
				}));

				matchAttributes.Add(new MatchAttribute("identifier[SSN].value", new List<double>
				{
					0, 1
				}));

				result = new ContentResult
				{
					Content = JsonSerializer.Serialize(matchAttributes, serializerOptions),
					ContentType = DefaultContentType,
					StatusCode = (int)HttpStatusCode.OK
				};
			}
			catch (Exception e)
			{
				this.logger.LogError($"Unable to retrieve configuration: {e}");
				result = new ContentResult
				{
					StatusCode = (int)HttpStatusCode.InternalServerError
				};
			}

			return result;
		}

		/// <summary>
		/// Retrieves ground truth scores asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration to be use to calculate the ground truth scores.</param>
		/// <returns>Returns the ground truth scores.</returns>
		[HttpGet]
		[Route("{id}/$groundTruthScores")]
		public async Task<IActionResult> GetGroundTruthScoresAsync(string id)
		{
			IActionResult result = this.BadRequest();

			if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
			{
				this.logger.LogError("The id value cannot be null or empty");
				return result;
			}

			try
			{
				await Task.Yield();

				var scores = new GroundTruthScores();

				scores.Matches.AddRange(Enumerable.Range(0, 5).Select(Convert.ToDouble).Select(c => c * new Random().NextDouble()));
				scores.NonMatches.AddRange(Enumerable.Range(0, 5).Select(Convert.ToDouble).Select(c => c * new Random().NextDouble()));

				result = new ContentResult
				{
					Content = JsonSerializer.Serialize(scores, serializerOptions),
					ContentType = DefaultContentType,
					StatusCode = (int)HttpStatusCode.OK
				};
			}
			catch (Exception e)
			{
				this.logger.LogError($"Unable to retrieve configuration: {e}");
				result = new ContentResult
				{
					StatusCode = (int)HttpStatusCode.InternalServerError
				};
			}

			return result;
		}

		/// <summary>
		/// Updates a match configuration asynchronously.
		/// </summary>
		/// <param name="id">The id of the match configuration to update.</param>
		/// <param name="resource">The resource.</param>
		/// <returns>Returns the updated match configuration.</returns>
		[HttpPut]
		[Route("{id}")]
		public async Task<IActionResult> UpdateAsync(string id, [FromBody] object resource)
		{
			IActionResult result = this.BadRequest();

			if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
			{
				this.logger.LogError("The id value cannot be null or empty");
				return result;
			}

			if (resource == null)
			{
				this.logger.LogError("The resource cannot be null");
				return result;
			}

			try
			{
				await Task.Yield();

				result = this.Ok();
			}
			catch (Exception e)
			{
				this.logger.LogError($"Unable to retrieve configuration: {e}");
				result = new ContentResult
				{
					StatusCode = (int)HttpStatusCode.InternalServerError
				};
			}

			return result;
		}
	}
}
