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
        /// Configuration reader.
        /// </summary>
        private readonly IConfigurationLoader ConfigurationLoader;

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Deployment actions that can be chosen by the user.
        /// </summary>
        private readonly List<IDeploymentAction> Actions;

        /// <summary>
        /// Change deployment mode action.
        /// </summary>
        private readonly DeploymentAction ChangeDeploymentModeAction;

        /// <summary>
        /// Deploy raw data action.
        /// </summary>
        private readonly DeployRawDataAction DeployRawDataAction;

        /// <summary>
        /// Exit action.
        /// </summary>
        private readonly DeploymentAction ExitAction;

        /// <summary>
        /// Tarkov data extraction action.
        /// </summary>
        private readonly ExtractTarkovDataAction ExtractTarkovDataAction;

        /// <summary>
        /// Initializes an new instance of the <see cref="Deployer"/> class.
        /// </summary>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="extractTarkovDataAction">Tarkov data extractor.</param>
        public Deployer(
            IConfigurationLoader configurationReader,
            IApplicationConfiguration configuration,
            DeployRawDataAction deployRawDataAction,
            ExtractTarkovDataAction extractTarkovDataAction)
        {
            Configuration = configuration;
            ConfigurationLoader = configurationReader;

            ChangeDeploymentModeAction = new DeploymentAction(
                () => $"Change deployment mode (current mode is \"{Configuration.ConfiguratorConfiguration.DeployerDeploymentMode}\")",
                ChooseDeploymentMode);
            DeployRawDataAction = deployRawDataAction;
            ExitAction = new DeploymentAction(
                "Exit",
                () => Task.CompletedTask);
            ExtractTarkovDataAction = extractTarkovDataAction;

            Actions = new List<IDeploymentAction>()
            {
                ChangeDeploymentModeAction,
                new DeploymentAction(
                    () => "TODO : Update Tarkov",
                    () => Task.CompletedTask),
                ExtractTarkovDataAction,
                DeployRawDataAction,
                new DeploymentAction(
                    () => "TODO : Deploy Azure Functions",
                    () => Task.CompletedTask),
                new DeploymentAction(
                    () => "TODO : Execute Azure Functions to update website data before scheduled time",
                    () => Task.CompletedTask),
                new DeploymentAction(
                    () => "TODO : Compile the website",
                    () => Task.CompletedTask),
                new DeploymentAction(
                    () => "TODO : Deploy the website to Azure",
                    () => Task.CompletedTask),
                ExitAction
            };
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            while (Configuration.ConfiguratorConfiguration.DeployerDeploymentMode == null)
            {
                await ChooseDeploymentMode();
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
                "Select an action",
                Actions.Select(a => a.Caption));

            Console.Clear();
            IDeploymentAction selectedAction = Actions.Single(a => a.Caption == choice);

            if (selectedAction == ExitAction)
            {
                return false;
            }

            await selectedAction.ExecuteAction();

            return true;
        }

        /// <summary>
        /// Displays the deployment mode selection and reloads the configuration matching the user's choice.
        /// </summary>
        private async Task ChooseDeploymentMode()
        {
            DeploymentMode choice = Prompt.Select(
                "Deployment mode",
                new DeploymentMode[]
                {
                   DeploymentMode.Test,
                   DeploymentMode.Production
                });

            if (choice == DeploymentMode.Production)
            {
                string confirmation = Prompt.Input<string>($"Confirm \"{DeploymentMode.Production.ToString().ToUpperInvariant()}\" deployment mode (type \"Yes\")");

                if (!string.Equals(confirmation, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            await ConfigurationLoader.Load(choice);
        }
    }
}
