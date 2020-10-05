using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using THNETII.CommandLine.Hosting;
using THNETII.SharePoint.IdentityModel;

namespace THNETII.SharePoint.AuthDiscoveryConsole
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var root = new RootCommand(CommandLineHost.GetEntryAssemblyDescription())
            { Handler = CommandLineHost.GetCommandHandler<ConsoleApplication>() };
            root.AddGlobalOption(new Option<string>(new[] { "--site", "-s" })
            {
                Name = nameof(SharePointAuthorizationDiscoveryOptions.SiteUrl),
                Description = "SharePoint site URL",
                Argument = { Name = "URL" }
            });

            var parser = new CommandLineBuilder(root)
                .UseDefaults()
                .UseHost(CreateHostBuilder)
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
                .PostConfigure<IHttpClientFactory>((opts, httpClientFactory) =>
                    opts.DiscoveryHttpClient = httpClientFactory.CreateClient())
                ;
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<SharePointAuthorizationDiscoveryOptions>, SharePointAuthorizationDiscoveryPostConfigureOptions>());
        }
    }
}
