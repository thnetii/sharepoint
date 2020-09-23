using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Hosting;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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

            AuthenticationResult? authResult;
            IEnumerable<string> scopes = Array.Empty<string>();
            var flow = pca
                .AcquireTokenWithDeviceCode(scopes, dcr =>
                {
                    var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                        .CreateLogger($"{typeof(DeviceCodeResult).Namespace}.{nameof(dcr.DeviceCode)}");
                    using var s_scps = BeginScope(logger, string.Join(" ", dcr.Scopes), nameof(dcr.Scopes));
                    using var s_usrc = BeginScope(logger, dcr.UserCode, nameof(dcr.UserCode));
                    using var s_vurl = BeginScope(logger, dcr.VerificationUrl, nameof(dcr.VerificationUrl));
                    using var s_expo = BeginScope(logger, dcr.ExpiresOn, nameof(dcr.ExpiresOn));
                    logger.LogInformation(dcr.Message);
                    return Task.CompletedTask;

                    static IDisposable BeginScope(ILogger logger, object value,
                        string name) =>
                        logger.BeginScope($"{name}: {{{name}}}", value);
                });

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
