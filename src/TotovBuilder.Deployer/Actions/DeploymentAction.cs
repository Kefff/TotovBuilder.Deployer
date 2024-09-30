using System;
using System.Threading.Tasks;
using TotovBuilder.Deployer.Abstractions.Actions;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents a deployment action.
    /// </summary>
    public class DeploymentAction : IDeploymentAction
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return GetCaptionFunction();
            }
        }

        /// <summary>
        /// Function for executing the action.
        /// </summary>
        private readonly Func<Task> ExecutionTask = () => Task.CompletedTask;

        /// <summary>
        /// Function for getting the caption to display in the menu.
        /// </summary>
        private readonly Func<string> GetCaptionFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentAction"/> class.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="executionTask"></param>
        public DeploymentAction(string caption, Func<Task>? executionTask = null)
            : this(() => caption, executionTask)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""/> class.
        /// </summary>
        /// <param name="getCaptionFunction"></param>
        /// <param name="executionTask"></param>
        public DeploymentAction(Func<string> getCaptionFunction, Func<Task>? executionTask = null)
        {
            GetCaptionFunction = getCaptionFunction;

            if (executionTask != null)
            {
                ExecutionTask = executionTask;
            }

        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        public async Task ExecuteAction()
        {
            await ExecutionTask();
        }
    }
}
