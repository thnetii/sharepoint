using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace THNETII.SharePoint.ShowCurrentUserConsole
{
    public static partial class Program
    {
        public static Task<int> Main(string[]? args)
        {
            var def = new CommandLineDefinition(typeof(CommandLineExecutor));
            var parser = new CommandLineBuilder(def.Command)
                .UseDefaults()
                .UseHost(CreateHostBuilder, def.ConfigureHost)
                .Build();
            return parser.InvokeAsync(args ?? Array.Empty<string>());
        }
    }
}
