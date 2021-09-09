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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SanteDB.ML.Adapter
{
	/// <summary>
	/// Represents the main program.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Defines the entry point of the application.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public static async Task Main(string[] args)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();

			var entryAssembly = Assembly.GetEntryAssembly();

#if DEBUG
			if (entryAssembly == null)
			{
				entryAssembly = typeof(Program).Assembly;
			}
#endif
			var workingDirectory = Path.GetDirectoryName(entryAssembly.Location);

			if (string.IsNullOrEmpty(workingDirectory) || string.IsNullOrWhiteSpace(workingDirectory))
			{
				throw new InvalidOperationException($"{nameof(workingDirectory)} cannot be null");
			}

			Directory.SetCurrentDirectory(workingDirectory);

			var shouldRunAsService = !(Debugger.IsAttached || args.Contains("--console"));

			var host = CreateHostBuilder(args).Build();

			var logger = host.Services.GetService<ILogger<Program>>();

			try
			{

				logger.LogInformation($"SanteDB ML Adapter: v{entryAssembly.GetName().Version}");
				logger.LogInformation($"SanteDB ML Adapter Working Directory : {Path.GetDirectoryName(entryAssembly.Location)}");
				logger.LogInformation($"Operating System: {Environment.OSVersion.Platform} {Environment.OSVersion.VersionString}");
				logger.LogInformation($"CLI Version: {Environment.Version}");
				logger.LogInformation($"{entryAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright}");

				var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

				applicationLifetime.ApplicationStarted.Register(() =>
				{
					stopwatch.Stop();
					logger.LogInformation($"Service started successfully in {stopwatch.Elapsed.TotalMilliseconds} ms");
				});

				applicationLifetime.ApplicationStopping.Register(() =>
				{
					logger.LogInformation("Service stopping");
				});

				applicationLifetime.ApplicationStopped.Register(() =>
				{
					logger.LogInformation("Service stopped");
				});


				if (OperatingSystem.IsWindows() && shouldRunAsService)
				{
					throw new NotImplementedException();
				}
				else if (OperatingSystem.IsWindows() && !shouldRunAsService)
				{
					await host.RunAsync();
				}
				else if (OperatingSystem.IsLinux())
				{
					await host.RunAsync();
				}
				else
				{
					throw new PlatformNotSupportedException($"Unsupported Platform: {Environment.OSVersion.Platform}");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"Unable to start service: {e}");
				throw;
			}
		}

		/// <summary>
		/// Creates the host builder.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <returns>Returns the host builder instance.</returns>
		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			var host = Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

					if (webBuilder.GetSetting("Environment") != "Development")
					{
						webBuilder.UseKestrel((context, options) =>
						{
							options.Configure(context.Configuration.GetSection("Kestrel"));
						});

					}

					webBuilder.UseStartup<Startup>();
				});

			return host;
		}
	}
}