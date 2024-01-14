using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer.Abstractions.Configuration
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
        /// Deployer configuration.
        /// </summary>
        DeployerConfiguration DeployerConfiguration { get; set; }
    }
}
