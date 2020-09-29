using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using THNETII.CommandLine.Hosting;

namespace THNETII.SharePoint.AuthDiscoveryConsole
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var definition = new RootCommandDefinition();

            var parser = new CommandLineBuilder(definition.Command)
                .UseDefaults()
                .UseHostingDefinition(definition, CreateHostBuilder)
                .Build();

            return parser.InvokeAsync(args ?? Array.Empty<string>());
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            args ??= Array.Empty<string>();

            var hostBuilder = CommandLineHost.CreateDefaultBuilder(args);
            hostBuilder.ConfigureServices(ConfigureServices);
            return hostBuilder;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddOptions<SharePointAuthorizationDiscoveryOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind(nameof(SharePoint), opts))
                .BindCommandLine()
                ;
        }
    }
}
