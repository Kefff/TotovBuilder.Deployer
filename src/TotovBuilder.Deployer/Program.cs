using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Actions;
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
            Console.WriteLine("TotovBuilder deployment tool");

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(typeof(ILogger<>), typeof(ApplicationLogger<>));
                    services.AddSingleton(typeof(IApplicationLogger<>), typeof(ApplicationLogger<>));
                    services.AddSingleton<IApplicationConfiguration, ApplicationConfiguration>();
                    services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
                    services.AddSingleton<IDeployer, Deployer>();
                    services.AddSingleton<DeployRawDataAction>();
                    services.AddSingleton<DeployWebsiteAction>();
                    services.AddSingleton<ExtractTarkovDataAction>();

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
