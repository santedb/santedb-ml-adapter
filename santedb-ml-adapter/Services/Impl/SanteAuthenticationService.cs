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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter.Services.Impl
{
	/// <summary>
	/// Represents a SanteDB authentication service.
	/// </summary>
	public class SanteAuthenticationService : ISanteAuthenticationService
	{
		/// <summary>
		/// The client identifier key.
		/// </summary>
		private const string ClientIdKey = "ClientId";

		/// <summary>
		/// The client secret key.
		/// </summary>
		private const string ClientSecretKey = "ClientSecret";

		/// <summary>
		/// The HTTP client factory.
		/// </summary>
		private readonly IHttpClientFactory clientFactory;

		/// <summary>
		/// The configuration.
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<SanteAuthenticationService> logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="SanteAuthenticationService" /> class.
		/// </summary>
		/// <param name="clientFactory">The client factory.</param>
		/// <param name="configuration">The configuration.</param>
		/// <param name="logger">The logger.</param>
		public SanteAuthenticationService(IHttpClientFactory clientFactory, IConfiguration configuration, ILogger<SanteAuthenticationService> logger)
		{
			this.clientFactory = clientFactory;
			this.configuration = configuration;
			this.logger = logger;
		}

		/// <summary>
		/// Authenticates against the SanteDB service.
		/// </summary>
		/// <returns>Returns the access token.</returns>
		public async Task<string> AuthenticateAsync()
		{
			var client = this.clientFactory.CreateClient();

			var clientId = this.configuration.GetValue<string>(ClientIdKey);
			var clientSecret = this.configuration.GetValue<string>(ClientSecretKey);

			if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
			{
				throw new InvalidOperationException("The client id and the client secret cannot be null or empty");
			}

			var response = await client.PostAsync($"{this.configuration.GetValue<string>("SanteDBEndpoint")}/auth/oauth2_token", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
			{
				new("grant_type", "client_credentials"),
				new("client_id", clientId),
				new("client_secret", clientSecret)
			}));

			var responseBody = await response.Content.ReadAsStringAsync();
			var content = JObject.Parse(responseBody);

			if (content.ContainsKey("error"))
			{
				this.logger.LogError($"Unable to authenticate against the SanteDB service: {responseBody}");
				throw new InvalidOperationException("Unable to authenticate against the SanteDB service");
			}


			var accessToken = content["access_token"]?.Value<string>();

			if (string.IsNullOrEmpty(accessToken) || string.IsNullOrWhiteSpace(accessToken))
			{
				this.logger.LogError($"Authentication response from the SanteDB service did not contain an 'access_token': {responseBody}");
				throw new InvalidOperationException("Authentication response from the SanteDB service did not contain an 'access_token'");
			}

			return accessToken;
		}

		/// <summary>
		/// Authenticates against the SanteDB service.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		/// <returns>Returns the access token.</returns>
		public async Task<string> AuthenticateAsync(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			if (parameters == null || !parameters.Any())
			{
				throw new ArgumentNullException(nameof(parameters), "Value cannot be null");
			}

			var client = this.clientFactory.CreateClient();

			var response = await client.PostAsync($"{this.configuration.GetValue<string>("SanteDBEndpoint")}/auth/oauth2_token", new FormUrlEncodedContent(parameters));

			var responseBody = await response.Content.ReadAsStringAsync();
			var content = JObject.Parse(responseBody);

			if (content.ContainsKey("error"))
			{
				this.logger.LogError($"Unable to authenticate against the SanteDB service: {responseBody}");
				throw new InvalidOperationException("Unable to authenticate against the SanteDB service");
			}

			var accessToken = content["access_token"]?.Value<string>();

			if (string.IsNullOrEmpty(accessToken) || string.IsNullOrWhiteSpace(accessToken))
			{
				this.logger.LogError($"Authentication response from the SanteDB service did not contain an 'access_token': {responseBody}");
				throw new InvalidOperationException("Authentication response from the SanteDB service did not contain an 'access_token'");
			}

			return accessToken;
		}
	}
}
