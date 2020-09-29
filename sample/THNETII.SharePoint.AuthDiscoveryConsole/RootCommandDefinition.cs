using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.SharePoint.AuthDiscoveryConsole
{
    internal class RootCommandDefinition
        : CommandLineHostingDefinition<RootCommandExecutor>
    {
        public RootCommandDefinition()
        {
            Command = new RootCommand(GetAssemblyDescription())
            { Handler = CommandHandler };

            SiteUrlOption = new Option<string>("--site", "SharePoint site URL")
            {
                Name = nameof(SharePointAuthorizationDiscoveryOptions.SiteUrl),
                Argument = { Name = "URL" }
            };
            SiteUrlOption.AddAlias("-s");
            Command.AddOption(SiteUrlOption);
        }

        public override Command Command { get; }
        public Option<string> SiteUrlOption { get; }
    }
}
