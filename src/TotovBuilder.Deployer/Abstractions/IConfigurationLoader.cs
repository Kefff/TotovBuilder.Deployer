using System.Threading.Tasks;
using TotovBuilder.Model;

namespace TotovBuilder.Deployer.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a configuration loader.
    /// </summary>
    public interface IConfigurationLoader
    {
        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <param name="deploymentMode">Deployment mode.</param>
        Task Load(DeploymentMode deploymentMode);
    }
}
