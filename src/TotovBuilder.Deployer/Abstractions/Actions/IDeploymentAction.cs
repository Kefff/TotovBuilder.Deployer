using System.Threading.Tasks;

namespace TotovBuilder.Deployer.Abstractions.Actions
{
    /// <summary>
    /// Provides the functionalities of a deployment action.
    /// </summary>
    public interface IDeploymentAction
    {
        /// <summary>
        /// Caption to display in the menu.
        /// Is also used to identify which action has been chosen by the user.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Executes the action.
        /// </summary>
        Task ExecuteAction();
    }

    /// <summary>
    /// Provides the functionalities of a deployment action.
    /// </summary>
    public interface IDeploymentAction<T> : IDeploymentAction
    {
    }
}
