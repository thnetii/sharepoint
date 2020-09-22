using System;
using System.CommandLine.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace THNETII.SharePoint.ShowCurrentUserConsole
{
    partial class Program
    {
        public static IHostBuilder CreateHostBuilder(string[]? args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args ?? Array.Empty<string>());

            // Insert embedded appsettings.json file at start of AppSettings
            // appseting.json read form current directory is placed LATER and
            // will thus override the embedded file (embedded provides defaults)
            hostBuilder.ConfigureAppConfiguration((context, config) =>
            {
                var fileProvider = new EmbeddedFileProvider(typeof(Program).Assembly);
                var hostingEnvironment = context.HostingEnvironment;

                var sources = config.Sources;
                int originalSourcesCount = sources.Count;

                config.AddJsonFile(fileProvider,
                    $"appsettings.json",
                    optional: true, reloadOnChange: true);
                config.AddJsonFile(fileProvider,
                    $"appsettings.{hostingEnvironment.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);

                const int insert_idx = 1;
                for (int i_dst = insert_idx, i_src = originalSourcesCount;
                    i_src < sources.Count; i_dst++, i_src++)
                {
                    var configSource = sources[i_src];
                    sources.RemoveAt(i_src);
                    sources.Insert(i_dst, configSource);
                }
            });

            hostBuilder.ConfigureServices(services =>
            {
                services.AddOptions<InvocationLifetimeOptions>()
                    .Configure<IConfiguration>((opts, config) =>
                        config.Bind("Lifetime", opts)
                        );
            });

            return hostBuilder;
        }
    }
}
