using System.Threading.Tasks;

namespace TotovBuilder.Deployer.Abstractions
{
    /// <summary>
    /// Provides the functionalities of the deployer.
    /// </summary>
    public interface IDeployer
    {
        /// <summary>
        /// Runs the deployer.
        /// </summary>
        Task Run();
    }
}
