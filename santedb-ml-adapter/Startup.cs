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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SanteDB.ML.Adapter.Auth;
using SanteDB.ML.Adapter.Services;
using SanteDB.ML.Adapter.Services.Impl;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter
{
	/// <summary>
	/// Represents the startup configuration settings for the application.
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// The web host webHostEnvironment.
		/// </summary>
		private readonly IWebHostEnvironment webHostEnvironment;

		/// <summary>
		/// The default authentication scheme.
		/// </summary>
		private const string DefaultAuthenticationScheme = "Basic";

		/// <summary>
		/// Initializes a new instance of the <see cref="Startup" /> class.
		/// </summary>
		/// <param name="webHostEnvironment">The web host webHostEnvironment.</param>
		/// <param name="configuration">The configuration.</param>
		/// <exception cref="ArgumentOutOfRangeException">webHostEnvironment - Invalid webHostEnvironment: Valid values are: Development, Staging, Production</exception>
		public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
		{
			this.Configuration = configuration;
			this.webHostEnvironment = webHostEnvironment;

			var environment = this.Configuration.GetValue<string>("Environment");

			switch (environment)
			{
				case "Development":
#if !DEBUG
					throw new ArgumentException($"Invalid webHostEnvironment: {webHostEnvironment} with a release build");
#endif
				case "Staging":
				case "Production":
					this.webHostEnvironment.EnvironmentName = environment;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(webHostEnvironment), $"Invalid Environment: {environment}. Valid values are: Development, Staging, Production");
			}
		}

		/// <summary>
		/// Gets the configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public IConfiguration Configuration { get; }

		/// <summary>
		/// Configures the services.
		/// This method gets called by the runtime. Use this method to add services to the container.
		/// </summary>
		/// <param name="services">The services.</param>
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = DefaultAuthenticationScheme;
				options.DefaultAuthenticateScheme = DefaultAuthenticationScheme;
				options.DefaultChallengeScheme = DefaultAuthenticationScheme;
			}).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(DefaultAuthenticationScheme, c =>
			{
				c.Validate();
			});

			services.AddAuthorization(options =>
			{
				options.DefaultPolicy = new AuthorizationPolicyBuilder(DefaultAuthenticationScheme)
					.RequireAuthenticatedUser()
					.Build();
			});

			services.AddControllers(options =>
			{
				options.RespectBrowserAcceptHeader = true;
				options.ReturnHttpNotAcceptable = true;
			});

			// add the HTTP client via DI
			services.AddHttpClient();

			// add support for HSTS
			services.AddHsts(options =>
			{
				options.Preload = true;
				options.IncludeSubDomains = true;
				options.MaxAge = TimeSpan.FromHours(24);

				if (this.webHostEnvironment.IsDevelopment())
				{
					options.MaxAge = TimeSpan.FromHours(1);
				}
			});

			// add the HTTP context to the pipeline to be accessed via DI
			services.AddHttpContextAccessor();

			// add host filtering based on the configuration of the allow hosts
			services.AddHostFiltering(options =>
			{
				options.AllowedHosts = this.Configuration.GetValue<string>("AllowedHosts").Split(';').ToList();
			});

			if (this.webHostEnvironment.IsDevelopment())
			{
				services.AddSwaggerGen(options =>
				{
					options.AddSecurityDefinition(DefaultAuthenticationScheme, new OpenApiSecurityScheme
					{
						Name = "Authorization",
						In = ParameterLocation.Header,
						Type = SecuritySchemeType.Http,
						Scheme = DefaultAuthenticationScheme
					});

					options.AddSecurityRequirement(new OpenApiSecurityRequirement
					{
						{
							new OpenApiSecurityScheme
							{
								Reference = new OpenApiReference
								{
									Type = ReferenceType.SecurityScheme,
									Id = DefaultAuthenticationScheme
								},
								Name = "Authorization",
								In = ParameterLocation.Header,
								Scheme = DefaultAuthenticationScheme
							},
							new List<string>()
						}
					});
				});
			}

			// add response compression
			services.AddResponseCompression(options =>
			{
				// this is explicitly disabled as it relates to CRIME
				// CRIME - https://en.wikipedia.org/wiki/CRIME
				// see also, https://en.wikipedia.org/wiki/BREACH
				options.EnableForHttps = false;
				options.Providers.Add(typeof(BrotliCompressionProvider));
				options.Providers.Add(typeof(GzipCompressionProvider));
			});

			// setup the response compression options
			services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
			services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });

			// add OUR services
			services.AddScoped<ISanteAuthenticationService, SanteAuthenticationService>();
			services.AddScoped<ISanteGroundTruthService, SanteGroundTruthService>();
			services.AddScoped<ISanteMatchConfigurationService, SanteMatchConfigurationService>();
			services.AddScoped<ISanteFhirMapService, SanteFhirMapService>();
		}

		/// <summary>
		/// Configures the specified application.
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app">The application.</param>
		/// <param name="environment">The webHostEnvironment.</param>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
		{
			var logger = app.ApplicationServices.GetService<ILogger<Startup>>();

			if (environment.IsDevelopment())
			{
				logger.LogInformation($"Environment is {environment.EnvironmentName}, will use the developer exception page");

				app.UseDeveloperExceptionPage();
			}
			else
			{
				logger.LogInformation($"Environment is {environment.EnvironmentName}, will enable HSTS");

				app.UseHsts();
			}

			// ensure that security headers are written into the response message
			app.Use(async (context, next) =>
			{
				// when the response writing begins
				context.Response.OnStarting(state =>
				{
					// deny the X-Frame-Options
					// indicates to the browser or another application, that this content should not be allowed to be included in a frame
					// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
					context.Response.Headers["X-Frame-Options"] = "DENY";

					// prevent "sniffing" of the content type
					// this prevents applications from guessing the content type of files or responses
					// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
					context.Response.Headers["X-Content-Type-Options"] = "nosniff";

					// block cross site scripting
					// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection
					context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

					// force the browser to not cache anything
					// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control
					context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";

					// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Pragma
					context.Response.Headers["Pragma"] = "no-cache";

					return Task.CompletedTask;
				}, context);

				// call the next middleware in the pipeline
				await next();
			});

			if (!environment.IsDevelopment())
			{
				//logger.LogInformation($"Environment is {environment.EnvironmentName}, will enable HTTPS redirection");

				//app.UseHttpsRedirection();
			}

			if (environment.IsDevelopment())
			{
				logger.LogInformation($"Environment is {environment.EnvironmentName}, will enable the OpenAPI test interface");

				app.UseSwagger();

				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
					c.RoutePrefix = string.Empty;
				});
			}

			logger.LogInformation("Enabling response compression");

			app.UseResponseCompression();

			logger.LogInformation("Enabling routing");

			app.UseRouting();

			logger.LogInformation("Enabling authentication and authorization");

			app.UseAuthentication();
			app.UseAuthorization();

			logger.LogInformation("Mapping controllers");

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			logger.LogInformation("Application configuration complete");
		}
	}
}
