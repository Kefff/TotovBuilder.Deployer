using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer.Abstractions
{
    /// <summary>
    /// Provides the functionalities of the application configuration.
    /// </summary>
    public interface IApplicationConfiguration
    {
        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration AzureFunctionsConfiguration { get; set; }

        /// <summary>
        /// Configurator configuration.
        /// </summary>
        DeployerConfiguration ConfiguratorConfiguration { get; set; }
    }
}
