using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Deployer.Utils;
using TotovBuilder.Deployer.Wrappers;
using TotovBuilder.Shared.Azure;
using TotovBuilder.Shared.Extensions;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents the application entry point.
    /// </summary>
    [ExcludeFromCodeCoverage()]
    public class Program
    {
        /// <summary>
        /// Executes the application.
        /// </summary>
        public async static Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(typeof(ILogger<>), typeof(ApplicationLogger<>));
                    services.AddSingleton(typeof(IApplicationLogger<>), typeof(ApplicationLogger<>));
                    services.AddSingleton<IApplicationConfiguration, ApplicationConfiguration>();
                    services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
                    services.AddSingleton<IConsoleWrapper, ConsoleWrapper>();
                    services.AddSingleton<IDeployer, Deployer>();
                    services.AddSingleton<IDirectoryWrapper, DirectoryWrapper>();
                    services.AddSingleton<IFileWrapper, FileWrapper>();
                    services.AddSingleton<IProcessWrapperFactory, ProcessWrapperFactory>();
                    services.AddSingleton<IPromtWrapper, PromptWrapper>();
                    services.AddSingleton<IStreamReaderWrapperFactory, StreamReaderWrapperFactory>();
                    services.AddSingleton<IStreamWriterWrapperFactory, StreamWriterWrapperFactory>();

                    services.AddSingleton<IDeploymentAction<CompileWebsiteAction>, CompileWebsiteAction>();
                    services.AddSingleton<IDeploymentAction<DeployRawDataAction>, DeployRawDataAction>();
                    services.AddSingleton<IDeploymentAction<DeployWebsiteAction>, DeployWebsiteAction>();
                    services.AddSingleton<IDeploymentAction<ExtractTarkovDataAction>, ExtractTarkovDataAction>();
                    services.AddSingleton<IDeploymentAction<UpdateTarkovAction>, UpdateTarkovAction>();

                    services.AddAzureBlobStorageManager(
                        (IServiceProvider serviceProvider) =>
                        {
                            IApplicationConfiguration configuration = serviceProvider.GetRequiredService<IApplicationConfiguration>();

                            return new AzureBlobStorageManagerOptions(configuration.AzureFunctionsConfiguration.AzureBlobStorageConnectionString, configuration.AzureFunctionsConfiguration.ExecutionTimeout);
                        });
                })
                .Build();

            IDeployer deployer = host.Services.GetRequiredService<IDeployer>();
            await deployer.Run();
        }
    }
}
