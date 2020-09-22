using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace THNETII.SharePoint.ShowCurrentUserConsole
{
    public class CommandLineDefinition
    {
        private readonly Type? executor;

        public CommandLineDefinition(Type? executor)
        {
            this.executor = executor;

            Command = new Command(GetDescription(executor))
            { Handler = GetCommandHandler(executor) };


        }

        public Command Command { get; }

        public void ConfigureHost(IHostBuilder host)
        {
            host.ConfigureServices(services =>
                services.AddSingleton(GetType(), this));

            const BindingFlags bfPubStatic = BindingFlags.Public |
                BindingFlags.Static | BindingFlags.IgnoreCase;
            var configureHostMethodInfo = executor?.GetMethod(
                nameof(ConfigureHost), bfPubStatic, Type.DefaultBinder,
                new[] { typeof(IHostBuilder) }, null);
            configureHostMethodInfo?.Invoke(null, new[] { host });
        }

        private static string GetDescription(Type? type)
        {
            string? description = null;
            if ((type?.Assembly ?? Assembly.GetEntryAssembly()) is { } programAssembly)
            {
                description = programAssembly
                    .GetCustomAttribute<AssemblyDescriptionAttribute>()?
                    .Description;
                description ??= programAssembly
                    .GetCustomAttribute<AssemblyProductAttribute>()?
                    .Product;
            }
            description ??= type?.Namespace;
            return description ?? string.Empty;
        }

        [return: NotNullIfNotNull("executor")]
        private static ICommandHandler? GetCommandHandler(
            Type? executor)
        {
            if (executor is null)
                return null;

            const BindingFlags bflags = BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.IgnoreCase;
            var runMethodInfo =
                executor.GetMethod("RunAsync", bflags) ??
                executor.GetMethod("Run", bflags);

            if (runMethodInfo is null)
                throw new ArgumentException(paramName: nameof(executor),
                    message: $"The specified type '{executor}' does not define an unambiguous method named either 'Run' or 'RunAsync'."
                    );

            if (runMethodInfo.IsStatic)
                return CommandHandler.Create(runMethodInfo);

            return CommandHandler.Create(
            async (InvocationContext context, IHost host) =>
            {
                using var serviceScope = host.Services.CreateScope();
                var target = ActivatorUtilities.GetServiceOrCreateInstance(
                    serviceScope.ServiceProvider, executor);
                var targetCommandHandler = CommandHandler
                    .Create(runMethodInfo, target);

                return await targetCommandHandler.InvokeAsync(context)
                    .ConfigureAwait(continueOnCapturedContext: false);
            });
        }
    }
}
