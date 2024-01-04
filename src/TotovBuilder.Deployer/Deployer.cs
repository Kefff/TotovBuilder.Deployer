using System;
using System.Threading.Tasks;
using Sharprompt;
using TotovBuilder.Deployer.Abstractions;
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
        /// Tarkov data extractor.
        /// </summary>
        private readonly ITarkovDataExtractor TarkovDataExtractor;

        /// <summary>
        /// Initializes an new instance of the <see cref="Deployer"/> class.
        /// </summary>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="tarkovDataExtractor">Tarkov data extractor.</param>
        public Deployer(
            IConfigurationLoader configurationReader,
            IApplicationConfiguration configuration,
            ITarkovDataExtractor tarkovDataExtractor)
        {
            Configuration = configuration;
            ConfigurationLoader = configurationReader;
            TarkovDataExtractor = tarkovDataExtractor;
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            while (Configuration.ConfiguratorConfiguration.DeployerDeploymentMode == null)
            {
                await DisplayDeploymentModeSelection();
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
            bool displayAgain = true;

            string deploymentModeSelectionOption = $"Change deployment mode (current mode is \"{Configuration.ConfiguratorConfiguration.DeployerDeploymentMode}\")";
            string extractionOption = "Extract missing item properties from Tarkov";

            string choice = Prompt.Select(
                "Select an action",
                new string[]
                {
                    deploymentModeSelectionOption,
                    "Update Tarkov",
                    extractionOption,
                    "Deploy raw data to Azure",
                    "Compile the website",
                    "Deploy the website to Azure",
                    "Exit"
                });

            if (choice == deploymentModeSelectionOption)
            {
                await DisplayDeploymentModeSelection();
            }
            else if (choice == extractionOption)
            {
                await TarkovDataExtractor.Extract();
            }
            else if (choice == "Exit")
            {
                displayAgain = false;
            }

            return displayAgain;
        }

        /// <summary>
        /// Displays the deployment mode selection.
        /// </summary>
        /// <returns>Chosen deployment mode.</returns>
        private async Task DisplayDeploymentModeSelection()
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
