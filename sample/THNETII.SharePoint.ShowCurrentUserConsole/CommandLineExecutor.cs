using System;
using System.CommandLine.Binding;
using System.CommandLine.Hosting;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MsalLogLevel = Microsoft.Identity.Client.LogLevel;

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


        }

        public static void ConfigureHost(IHostBuilder host)
        {
            _ = host ?? throw new ArgumentNullException(nameof(host));

            host.ConfigureServices(services =>
            {
                services.AddHttpClient<IMsalHttpClientFactory, MsalHttpClient>();
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
                services.AddTransient(serviceProvider =>
                {
                    var logger = serviceProvider
                        .GetRequiredService<ILogger<IPublicClientApplication>>();
                    var msalHttpFacory = serviceProvider
                        .GetRequiredService<IMsalHttpClientFactory>();
                    var options = serviceProvider
                        .GetRequiredService<IOptions<PublicClientApplicationOptions>>()
                        .Value;
                    return PublicClientApplicationBuilder
                        .CreateWithApplicationOptions(options)
                        .WithHttpClientFactory(msalHttpFacory)
                        .WithLogging((logLevel, message, containsPii) =>
                        {
                            var msExtLogLevel = logLevel switch
                            {
                                MsalLogLevel.Error => LogLevel.Error,
                                MsalLogLevel.Warning => LogLevel.Warning,
                                MsalLogLevel.Info => LogLevel.Information,
                                MsalLogLevel.Verbose => LogLevel.Debug,
                                _ => LogLevel.Trace
                            };
                            logger.Log(msExtLogLevel, message);
                        }, MsalLogLevel.Verbose)
                        .Build();
                });
            });
        }
    }

    public class MsalHttpClient : IMsalHttpClientFactory
    {
        private readonly HttpClient httpClient;

        public MsalHttpClient(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public HttpClient GetHttpClient() => httpClient;
    }
}
