using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer
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
