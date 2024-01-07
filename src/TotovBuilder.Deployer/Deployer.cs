using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sharprompt;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Abstractions.Actions;
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
        /// Exit action.
        /// </summary>
        private readonly DeploymentAction ExitAction;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<Deployer> Logger;

        /// <summary>
        /// Initializes an new instance of the <see cref="Deployer"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="compileWebsiteAction">Action to compile the Totov Builder website.</param>
        /// <param name="deployRawDataAction">Action to deploy to Azure raw data used by Azure Functions to generated website data.</param>
        /// <param name="deployWebsiteAction">Action to deploy the website to Azure.</param>
        /// <param name="extractTarkovDataAction">Action to extract missing item properties from Tarkov data.</param>
        /// <param name="launchTarkovAction">Action to launch the Escape from Tarkov launcher to update the game.</param>
        public Deployer(
            IApplicationLogger<Deployer> logger,
            IConfigurationLoader configurationReader,
            IApplicationConfiguration configuration,
            CompileWebsiteAction compileWebsiteAction,
            DeployRawDataAction deployRawDataAction,
            DeployWebsiteAction deployWebsiteAction,
            ExtractTarkovDataAction extractTarkovDataAction,
            UpdateTarkovAction launchTarkovAction)
        {
            Configuration = configuration;
            ConfigurationLoader = configurationReader;
            Logger = logger;

            ExitAction = new DeploymentAction(
                Properties.Resources.ExitAction,
                () => Task.CompletedTask);

            Actions = new List<IDeploymentAction>()
            {
                launchTarkovAction,
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
            string choice = Prompt.Select(
                Properties.Resources.SelectedAction,
                Actions.Select(a => a.Caption));

            DisplayTitle();
            DisplayCurrentDeploymentMode();

            IDeploymentAction selectedAction = Actions.Single(a => a.Caption == choice);

            if (selectedAction == ExitAction)
            {
                return false;
            }

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
        /// Displays the deployment mode selection and reloads the configuration matching the user's choice.
        /// </summary>
        /// <returns><c>true</c> when the configuration matching the chosen deployment mode is loaded; otherwise false.</returns>
        private async Task<bool> ChooseDeploymentMode()
        {
            DeploymentMode choice = Prompt.Select(
                Properties.Resources.DeploymentMode,
                new DeploymentMode[]
                {
                   DeploymentMode.Test,
                   DeploymentMode.Production
                });

            if (choice == DeploymentMode.Production)
            {
                string confirmation = Prompt.Input<string>(string.Format(Properties.Resources.ConfirmDeploymentMode, DeploymentMode.Production.ToString().ToUpperInvariant()));

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
        private static Task DisplayActionInstructions(string instructions)
        {
            ConsoleColor originalForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(instructions);

            Console.ForegroundColor = originalForegroundColor;
            Console.WriteLine();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays the current deployment mode.
        /// </summary>
        private void DisplayCurrentDeploymentMode()
        {
            ConsoleColor originalForegroundColor = Console.ForegroundColor;
            Console.Write(Properties.Resources.CurrentDeploymentMode);

            switch (Configuration.DeployerConfiguration.DeployerDeploymentMode)
            {
                case DeploymentMode.Production:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case DeploymentMode.Test:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Console.WriteLine(Configuration.DeployerConfiguration.DeployerDeploymentMode.ToString()!.ToUpperInvariant());

            Console.ForegroundColor = originalForegroundColor;
            Console.WriteLine();
        }

        /// <summary>
        /// Displays the application title.
        /// </summary>
        private static void DisplayTitle()
        {
            ConsoleColor originalForegroundColor = Console.ForegroundColor;

            Console.Clear();
            Console.ForegroundColor = Console.BackgroundColor;
            Console.Write(Properties.Resources.Title_Part1);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Properties.Resources.Title_Part2);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(Properties.Resources.Title_Part3);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(Properties.Resources.Title_Part4);

            Console.ForegroundColor = Console.BackgroundColor;
            Console.Write(Properties.Resources.Title_Part5);

            Console.ForegroundColor = originalForegroundColor;
            Console.WriteLine();
        }
    }
}
