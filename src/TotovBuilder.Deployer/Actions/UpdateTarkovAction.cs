using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to launch the Escape from Tarkov launcher to update the game.
    /// </summary>
    public class UpdateTarkovAction : IDeploymentAction<UpdateTarkovAction>
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return Properties.Resources.UpdateTarkovAction;
            }
        }

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<UpdateTarkovAction> Logger;

        /// <summary>
        /// Process wrapper factory.
        /// </summary>
        private readonly IProcessWrapperFactory ProcessWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateTarkovAction"/> class.
        /// </summary>
        /// <param name="Logger">Logger.</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="processWrapperFactory">Process wrapper factory.</param>
        public UpdateTarkovAction(IApplicationLogger<UpdateTarkovAction> logger, IApplicationConfiguration configuration, IProcessWrapperFactory processWrapperFactory)
        {
            Configuration = configuration;
            Logger = logger;
            ProcessWrapperFactory = processWrapperFactory;
        }

        /// <inheritdoc/>
        public Task ExecuteAction()
        {
            Logger.LogInformation(string.Format(Properties.Resources.StartingTarkovLauncher, Configuration.DeployerConfiguration.TarkovLauncherExecutableFilePath));

            using (IProcessWrapper processWrapper = ProcessWrapperFactory.Create())
            {
                processWrapper.StartInfo.FileName = Configuration.DeployerConfiguration.TarkovLauncherExecutableFilePath;
                processWrapper.StartInfo.CreateNoWindow = true;
                processWrapper.Start();
            }

            Logger.LogSuccess(Properties.Resources.TarkovLauncherStarted);

            return Task.CompletedTask;
        }
    }
}
