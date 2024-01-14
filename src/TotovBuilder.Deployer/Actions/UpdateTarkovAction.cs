using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Logs;

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
        /// Initializes a new instance of the <see cref="UpdateTarkovAction"/> class.
        /// </summary>
        /// <param name="Logger">Logger.</param>
        /// <param name="configuration">Configuration</param>
        public UpdateTarkovAction(IApplicationLogger<UpdateTarkovAction> logger, IApplicationConfiguration configuration)
        {
            Configuration = configuration;
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task ExecuteAction()
        {
            Logger.LogInformation(string.Format(Properties.Resources.StartingTarkovLauncher, Configuration.DeployerConfiguration.TarkovLauncherExecutableFilePath));

            Process process = new Process();
            process.StartInfo.FileName = Configuration.DeployerConfiguration.TarkovLauncherExecutableFilePath;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            Logger.LogSuccess(Properties.Resources.TarkovLauncherStarted);

            return Task.CompletedTask;
        }
    }
}
