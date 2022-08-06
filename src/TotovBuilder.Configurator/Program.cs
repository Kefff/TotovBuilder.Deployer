using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using TotovBuilder.Configurator.Abstractions;

namespace TotovBuilder.Configurator
{
    /// <summary>
    /// Represents the application entry point.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Executes the application.
        /// </summary>
        public async static Task Main(string[] args)
        {
            bool upload = args.Contains("-u") || args.Contains("--upload");
            
            try
            {
                IConfigurationReader configurationReader = new ConfigurationReader();
                IConfigurator configurator = new Configurator(
                    configurationReader,
                    new TarkovDataExtractor(configurationReader),
                    upload ? new AzureBlobDataUploader(configurationReader) : null);
                await configurator.Execute();
            }            
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }
    }
}
