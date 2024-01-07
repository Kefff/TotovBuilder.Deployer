using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Model;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents a configuration reader.
    /// </summary>
    public class ConfigurationLoader : IConfigurationLoader
    {
        private const string ConfigurationsDirectoryKey = "ConfigurationsDirectory";
        private const string DeployerConfigurationFileNameKey = "DeployerConfigurationFileName";

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<ConfigurationLoader> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationLoader"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Application configuration.</param>
        public ConfigurationLoader(IApplicationLogger<ConfigurationLoader> logger, IApplicationConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task Load(DeploymentMode deploymentMode)
        {
            string configurationsDirectory = Path.Combine(ConfigurationManager.AppSettings.Get(ConfigurationsDirectoryKey)!, deploymentMode.ToString().ToUpper());
            string deployerConfigurationFileName = ConfigurationManager.AppSettings.Get(DeployerConfigurationFileNameKey)!;
            string deployerConfigurationFilePath = Path.Combine(configurationsDirectory, deployerConfigurationFileName);

            Logger.LogInformation(string.Format(Properties.Resources.LoadingDeployerConfiguration, deployerConfigurationFilePath));

            string deployerConfigurationJson = await File.ReadAllTextAsync(deployerConfigurationFilePath);
            Configuration.DeployerConfiguration = JsonSerializer.Deserialize<DeployerConfiguration>(deployerConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;
            Configuration.DeployerConfiguration.DeployerDeploymentMode = deploymentMode;
            Configuration.DeployerConfiguration.ConfigurationsDirectory = configurationsDirectory;
            Configuration.DeployerConfiguration.DeployerConfigurationFileName = deployerConfigurationFileName;

            string azureFunctionsConfigurationFilePath = Path.Combine(Configuration.DeployerConfiguration.ConfigurationsDirectory, Configuration.DeployerConfiguration.AzureFunctionsConfigurationBlobName);

            Logger.LogInformation(string.Format(Properties.Resources.LoadingAzureFunctionsConfiguration, azureFunctionsConfigurationFilePath));

            string azureFunctionsConfigurationJson = await File.ReadAllTextAsync(azureFunctionsConfigurationFilePath);
            Configuration.AzureFunctionsConfiguration = JsonSerializer.Deserialize<AzureFunctionsConfiguration>(azureFunctionsConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;

            Logger.LogSuccess(string.Format(Properties.Resources.ConfigurationLoaded));
        }
    }
}
