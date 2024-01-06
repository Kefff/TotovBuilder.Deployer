using System.Threading.Tasks;
using FluentResults;
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
        /// <returns><c>true</c> when the configuration is loaded; otherwise false.</returns>
        Task<bool> Load(DeploymentMode deploymentMode);
    }
}
