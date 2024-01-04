using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TotovBuilder.Deployer.Abstractions;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents the application entry point.
    /// </summary>
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
                    services.AddSingleton<IApplicationConfiguration, ApplicationConfiguration>();
                    services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
                    services.AddSingleton<IDeployer, Deployer>();
                    services.AddSingleton<ITarkovDataExtractor, TarkovDataExtractor>();
                })
                .Build();
            
            IDeployer deployer = host.Services.GetRequiredService<IDeployer>();
            await deployer.Run();
        }
    }
}
