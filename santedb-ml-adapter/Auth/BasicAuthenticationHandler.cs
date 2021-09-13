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
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter.Auth
{
	/// <summary>
	/// Represents a basic authentication handler.
	/// </summary>
	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		/// <summary>
		/// The configuration.
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger<BasicAuthenticationHandler> logger;

		/// <summary>
		/// The authorization error messages.
		/// </summary>
		private static readonly Dictionary<string, string> authorizationErrorMessages = new()
		{
			{ "AUTH_ERR_001", "No Authorization header present" },
			{ "AUTH_ERR_002", "Invalid scheme" },
			{ "AUTH_ERR_003", "Invalid credentials" }
		};

		/// <summary>
		/// Initializes a new instance of <see cref="BasicAuthenticationHandler" /> class.
		/// </summary>
		/// <param name="options">The monitor for the options instance.</param>
		/// <param name="loggerFactory">The <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" />.</param>
		/// <param name="encoder">The <see cref="T:System.Text.Encodings.Web.UrlEncoder" />.</param>
		/// <param name="clock">The <see cref="T:Microsoft.AspNetCore.Authentication.ISystemClock" />.</param>
		/// <param name="configuration">The configuration.</param>
		/// <param name="logger">The logger.</param>
		public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder, ISystemClock clock, IConfiguration configuration, ILogger<BasicAuthenticationHandler> logger)
			: base(options, loggerFactory, encoder, clock)
		{
			this.configuration = configuration;
			this.logger = logger;
		}

		/// <summary>
		/// Handles the basic authentication for the application.
		/// </summary>
		/// <returns>Returns an authentication result representing the outcome of the authentication operation.</returns>
		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			await Task.Yield();


			foreach (var (key, value) in this.Request.Headers)
			{
				this.logger.LogDebug($"{key}: {value}");
			}
			

			if (!this.Request.Headers.ContainsKey("Authorization"))
			{
				this.logger.LogError("Authorization header missing");
				return AuthenticateResult.Fail(authorizationErrorMessages["AUTH_ERR_001"]);
			}

			try
			{
				var header = this.Request.Headers["Authorization"][0];

				if (string.IsNullOrEmpty(header) || string.IsNullOrWhiteSpace(header))
				{
					this.logger.LogError("No value present for Authorization header");
					return AuthenticateResult.Fail(authorizationErrorMessages["AUTH_ERR_002"]);
				}

				if (!header.StartsWith("Basic "))
				{
					this.logger.LogError("Authorization header must start with the scheme of Basic");
					return AuthenticateResult.Fail(authorizationErrorMessages["AUTH_ERR_002"]);
				}

				var headerValue = header["Basic ".Length..];

				if (string.IsNullOrEmpty(headerValue) || string.IsNullOrWhiteSpace(headerValue))
				{
					this.logger.LogError("No value present for the Basic scheme");
					return AuthenticateResult.Fail(authorizationErrorMessages["AUTH_ERR_003"]);
				}

				if (this.configuration.GetValue<string>("Key") == headerValue)
				{
					return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
					{
						new(ClaimTypes.Sid, "machine learning")
					}, "Basic")), new AuthenticationProperties
					{
						IssuedUtc = DateTimeOffset.UtcNow
					}, "Basic"));
				}

				this.logger.LogError("Invalid credentials");
				return AuthenticateResult.Fail(authorizationErrorMessages["AUTH_ERR_003"]);
			}
			catch (Exception e)
			{
				this.logger.LogError($"Unable to perform authentication check: {e}");
				return AuthenticateResult.Fail(authorizationErrorMessages["AUTH_ERR_003"]);
			}
		}
	}
}
