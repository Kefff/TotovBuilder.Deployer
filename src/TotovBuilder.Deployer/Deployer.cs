using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Logs;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Model;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents the deployer.
    /// </summary>
    public class Deployer : IDeployer
    {
        /// <summary>
        /// Deployment actions that can be chosen by the user.
        /// </summary>
        private readonly List<IDeploymentAction> Actions;

        /// <summary>
        /// Configuration reader.
        /// </summary>
        private readonly IConfigurationLoader ConfigurationLoader;

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Console wrapper.
        /// </summary>
        private readonly IConsoleWrapper ConsoleWrapper;

        /// <summary>
        /// Exit action.
        /// </summary>
        private readonly DeploymentAction ExitAction;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<Deployer> Logger;

        /// <summary>
        /// Prompt wrapper.
        /// </summary>
        private readonly IPromtWrapper PromptWrapper;

        /// <summary>
        /// Initializes an new instance of the <see cref="Deployer"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="consoleWrapper">Console wrapper.</param>
        /// <param name="promptWrapper">Prompt wrapper.</param>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="compileWebsiteAction">Action to compile the Totov Builder website.</param>
        /// <param name="deployRawDataAction">Action to deploy to Azure raw data used by Azure Functions to generated website data.</param>
        /// <param name="deployWebsiteAction">Action to deploy the website to Azure.</param>
        /// <param name="extractTarkovDataAction">Action to extract missing item properties from Tarkov data.</param>
        /// <param name="updateTarkovAction">Action to launch the Escape from Tarkov launcher to update the game.</param>
        public Deployer(
            IApplicationLogger<Deployer> logger,
            IConsoleWrapper consoleWrapper,
            IPromtWrapper promptWrapper,
            IConfigurationLoader configurationLoader,
            IApplicationConfiguration configuration,
            IDeploymentAction<CompileWebsiteAction> compileWebsiteAction,
            IDeploymentAction<DeployRawDataAction> deployRawDataAction,
            IDeploymentAction<DeployWebsiteAction> deployWebsiteAction,
            IDeploymentAction<ExtractTarkovDataAction> extractTarkovDataAction,
            IDeploymentAction<UpdateTarkovAction> updateTarkovAction)
        {
            Configuration = configuration;
            ConfigurationLoader = configurationLoader;
            ConsoleWrapper = consoleWrapper;
            Logger = logger;
            PromptWrapper = promptWrapper;

            ExitAction = new DeploymentAction(
                Properties.Resources.ExitAction,
                () => Task.CompletedTask);

            Actions = new List<IDeploymentAction>()
            {
                updateTarkovAction,
                extractTarkovDataAction,
                new DeploymentAction(
                    () => Properties.Resources.UpdateChangelogAction,
                    () => DisplayActionInstructions(Properties.Resources.UpdateChangelogInstructions)),
                new DeploymentAction(
                    () => Properties.Resources.CheckConfigurationFilesAction,
                    () => DisplayActionInstructions(Properties.Resources.CheckConfigurationFilesInstructions)),
                compileWebsiteAction,
                deployRawDataAction,
                new DeploymentAction(
                    () => Properties.Resources.DeployAzureFunctionsAction,
                    () => DisplayActionInstructions(Properties.Resources.DeployAzureFunctionsInstructions)),
                deployWebsiteAction,
                new DeploymentAction(
                    () => Properties.Resources.PurgeCdnAction,
                    () => DisplayActionInstructions(Properties.Resources.PurgeCdnInstructions)),
                new DeploymentAction(
                    () => Properties.Resources.CheckWebsiteAction,
                    () => DisplayActionInstructions(Properties.Resources.CheckWebsiteInstructions)),
                new DeploymentAction(
                    () => Properties.Resources.GitAction,
                    () => DisplayActionInstructions(Properties.Resources.GitInstructions)),
                new DeploymentAction(
                    () => Properties.Resources.DiscordAction,
                    () => DisplayActionInstructions(Properties.Resources.DiscordInstructions)),
                new DeploymentAction(
                    () => Properties.Resources.ChangeDeploymentModeAction,
                    ChooseDeploymentMode),
                ExitAction
            };
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            bool isConfigurationLoaded = false;

            DisplayTitle();

            while (!isConfigurationLoaded)
            {
                isConfigurationLoaded = await ChooseDeploymentMode();
            }

            bool displayMenu = true;

            while (displayMenu)
            {
                displayMenu = await DisplayMenu();
            }
        }

        /// <summary>
        /// Displays the menu for the user to choose an action.
        /// </summary>
        /// <returns><c>true</c> when the menu must be displayed again after the action is executed; otherwise <c>false</c>.</returns>
        private async Task<bool> DisplayMenu()
        {
            string choice = PromptWrapper.Select(
                Properties.Resources.SelectedAction,
                Actions.Select(a => a.Caption));

            DisplayTitle();
            DisplayCurrentDeploymentMode();

            if (choice == ExitAction.Caption)
            {
                return false;
            }

            IDeploymentAction selectedAction = Actions.Single(a => a.Caption == choice);

            try
            {
                await selectedAction.ExecuteAction();
            }
            catch (Exception e)
            {
                string error = e.ToString();
                Logger.LogError(error);
            }

            return true;
        }

        /// <summary>
        /// Displays the deployment mode selection and loads the configuration matching the user's choice.
        /// </summary>
        /// <returns><c>true</c> when the configuration matching the chosen deployment mode is loaded; otherwise false.</returns>
        private async Task<bool> ChooseDeploymentMode()
        {
            DeploymentMode choice = PromptWrapper.Select(
                Properties.Resources.DeploymentMode,
                new DeploymentMode[]
                {
                   DeploymentMode.Test,
                   DeploymentMode.Production
                });

            if (choice == DeploymentMode.Production)
            {
                string confirmation = PromptWrapper.Input<string>(string.Format(Properties.Resources.ConfirmDeploymentMode, DeploymentMode.Production.ToString().ToUpperInvariant()));

                if (!string.Equals(confirmation, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            try
            {
                await ConfigurationLoader.Load(choice);
                DisplayCurrentDeploymentMode();

                return true;
            }
            catch (Exception e)
            {
                string error = e.ToString();
                Logger.LogError(error);

                return false;
            }
        }

        /// <summary>
        /// Displays in yellow the instructions of an action.
        /// </summary>
        /// <param name="instructions">Instructions.</param>
        private Task DisplayActionInstructions(string instructions)
        {
            ConsoleColor originalForegroundColor = ConsoleWrapper.ForegroundColor;

            ConsoleWrapper.ForegroundColor = ConsoleColor.Yellow;
            ConsoleWrapper.WriteLine(instructions);

            ConsoleWrapper.ForegroundColor = originalForegroundColor;
            ConsoleWrapper.WriteLine();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays the current deployment mode.
        /// </summary>
        private void DisplayCurrentDeploymentMode()
        {
            ConsoleColor originalForegroundColor = ConsoleWrapper.ForegroundColor;
            ConsoleWrapper.Write(Properties.Resources.CurrentDeploymentMode);

            switch (Configuration.DeployerConfiguration.DeployerDeploymentMode)
            {
                case DeploymentMode.Production:
                    ConsoleWrapper.ForegroundColor = ConsoleColor.Red;
                    break;
                case DeploymentMode.Test:
                    ConsoleWrapper.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            ConsoleWrapper.WriteLine(Configuration.DeployerConfiguration.DeployerDeploymentMode.ToString()!.ToUpperInvariant());

            ConsoleWrapper.ForegroundColor = originalForegroundColor;
            ConsoleWrapper.WriteLine();
        }

        /// <summary>
        /// Displays the application title.
        /// </summary>
        private void DisplayTitle()
        {
            ConsoleColor originalForegroundColor = ConsoleWrapper.ForegroundColor;

            ConsoleWrapper.Clear();
            ConsoleWrapper.ForegroundColor = ConsoleWrapper.BackgroundColor;
            ConsoleWrapper.Write(Properties.Resources.Title_Part1);

            ConsoleWrapper.ForegroundColor = ConsoleColor.White;
            ConsoleWrapper.Write(Properties.Resources.Title_Part2);

            ConsoleWrapper.ForegroundColor = ConsoleColor.Red;
            ConsoleWrapper.Write(Properties.Resources.Title_Part3);

            ConsoleWrapper.ForegroundColor = ConsoleColor.DarkGray;
            ConsoleWrapper.Write(Properties.Resources.Title_Part4);

            ConsoleWrapper.ForegroundColor = ConsoleWrapper.BackgroundColor;
            ConsoleWrapper.Write(Properties.Resources.Title_Part5);

            ConsoleWrapper.ForegroundColor = originalForegroundColor;
            ConsoleWrapper.WriteLine();
        }
    }
}
