using System.Threading.Tasks;

namespace TotovBuilder.Configurator.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a configurator.
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Executes the configurator.
        /// </summary>
        Task Execute();
    }
}
