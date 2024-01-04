using System.Threading.Tasks;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a configuration reader.
    /// </summary>
    public interface IConfigurationReader
    {
        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration AzureFunctionsConfiguration { get; }

        /// <summary>
        /// Configurator configuration.
        /// </summary>
        ConfiguratorConfiguration ConfiguratorConfiguration { get; }

        /// <summary>
        /// Waits for the configuration to be loaded.
        /// </summary>
        Task WaitForLoading();
    }
}
