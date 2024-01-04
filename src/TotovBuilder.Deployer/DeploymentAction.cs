using System;
using System.Threading.Tasks;
using TotovBuilder.Deployer.Abstractions.Actions;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents a deployment action.
    /// </summary>
    public class DeploymentAction : IDeploymentAction
    {
        /// <summary>
        /// Function for getting the caption to display in the menu.
        /// Is also used to identify which action has been chosen by the user.
        /// </summary>
        public string Caption
        {
            get
            {
                return _getCaptionFunction();
            }
        }
        private readonly Func<string> _getCaptionFunction;

        /// <summary>
        /// Function for executing the action.
        /// </summary>
        private readonly Func<Task> ExecutionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref=""/> class.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="executionTask"></param>
        public DeploymentAction(string caption, Func<Task> executionTask)
            : this(() => caption, executionTask)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=""/> class.
        /// </summary>
        /// <param name="getCaptionFunction"></param>
        /// <param name="executionTask"></param>
        public DeploymentAction(Func<string> getCaptionFunction, Func<Task> executionTask)
        {
            ExecutionTask = executionTask;
            _getCaptionFunction = getCaptionFunction;

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
