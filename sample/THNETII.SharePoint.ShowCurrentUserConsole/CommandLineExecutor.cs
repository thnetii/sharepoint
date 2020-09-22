using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Hosting;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

using THNETII.MsalExtensions.Hosting;

namespace THNETII.SharePoint.ShowCurrentUserConsole
{
    public static class CommandLineExecutor
    {
        public static async Task RunAsync(IHost host,
            CancellationToken cancelToken = default)
        {
            _ = host ?? throw new ArgumentNullException(nameof(host));

            using var serviceScope = host.Services.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;

            var pca = serviceProvider.GetRequiredService<IPublicClientApplication>();

            var flow = pca
                .AcquireTokenWithDeviceCode(Array.Empty<string>(),
                deviceCodeResult =>
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<DeviceCodeResult>>();
                    using var loggerScope = logger.BeginScope(new Dictionary<string, string>
                    {
                        [nameof(deviceCodeResult.ClientId)] = deviceCodeResult.ClientId,
                        [nameof(deviceCodeResult.DeviceCode)] = deviceCodeResult.DeviceCode,
                        [nameof(deviceCodeResult.ExpiresOn)] = deviceCodeResult.ExpiresOn.ToString(CultureInfo.InvariantCulture),
                        [nameof(deviceCodeResult.Interval)] = deviceCodeResult.Interval.ToString(CultureInfo.InvariantCulture),
                        [nameof(deviceCodeResult.Scopes)] = string.Join(" ", deviceCodeResult.Scopes),
                        [nameof(deviceCodeResult.UserCode)] = deviceCodeResult.UserCode,
                        [nameof(deviceCodeResult.VerificationUrl)] = deviceCodeResult.VerificationUrl,
                    });
                    logger.LogInformation(deviceCodeResult.Message);
                    return Task.CompletedTask;
                });

            AuthenticationResult authResult;
            try
            {
                authResult = await flow.ExecuteAsync(cancelToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (OperationCanceledException cancelExcept)
            {
                serviceProvider.GetRequiredService<ILoggerFactory>()
                    .CreateLogger(typeof(CommandLineExecutor))
                    .LogCritical(cancelExcept, cancelExcept.Message);
                return;
            }

            _ = authResult;
        }

        public static void ConfigureHost(IHostBuilder host)
        {
            _ = host ?? throw new ArgumentNullException(nameof(host));

            host.ConfigureServices(services =>
            {
                services.AddMsalNet();
                services.AddSingleton(serviceProvider =>
                {
                    var definition = serviceProvider.GetRequiredService<CommandLineDefinition>();
                    var modelBinder = new ModelBinder<PublicClientApplicationOptions>()
                    { EnforceExplicitBinding = true };



                    return modelBinder;
                });
                services.AddOptions<PublicClientApplicationOptions>()
                    .Configure<IConfiguration>((opts, config) =>
                        config.Bind(typeof(PublicClientApplicationOptions).Namespace, opts)
                        )
                    .BindCommandLine()
                    ;
            });
        }
    }
}
