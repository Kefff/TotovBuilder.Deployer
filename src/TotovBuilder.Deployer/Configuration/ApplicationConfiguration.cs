using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer.Configuration
{
    /// <summary>
    /// Represents the application configuration.
    /// </summary>
    public class ApplicationConfiguration : IApplicationConfiguration
    {
        /// <inheritdoc/>
        public AzureFunctionsConfiguration AzureFunctionsConfiguration { get; set; } = new AzureFunctionsConfiguration();

        /// <inheritdoc/>
        public DeployerConfiguration DeployerConfiguration { get; set; } = new DeployerConfiguration();
    }
}
