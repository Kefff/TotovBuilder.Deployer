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
        private readonly ILogger<ConfigurationLoader> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationLoader"/> class.
        /// </summary>
        public ConfigurationLoader(ILogger<ConfigurationLoader> logger, IApplicationConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        /// <summary>
        /// Waits for the configuration to be loaded.
        /// </summary>
        /// <param name="deploymentMode">Deployment mode.</param>
        public async Task Load(DeploymentMode deploymentMode)
        {
            string configurationsDirectory = Path.Combine(ConfigurationManager.AppSettings.Get(ConfigurationsDirectoryKey)!, deploymentMode.ToString().ToUpper());
            string deployerConfigurationFileName = ConfigurationManager.AppSettings.Get(DeployerConfigurationFileNameKey)!;
            string deployerConfigurationFilePath = Path.Combine(configurationsDirectory, deployerConfigurationFileName);

            Logger.LogInformation(string.Format(Properties.Resources.DeployerConfigurationLoading, deployerConfigurationFilePath));

            string deployerConfigurationJson = await File.ReadAllTextAsync(deployerConfigurationFilePath);
            Configuration.ConfiguratorConfiguration = JsonSerializer.Deserialize<DeployerConfiguration>(deployerConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })! ;
            Configuration.ConfiguratorConfiguration.DeployerDeploymentMode = deploymentMode;
            Configuration.ConfiguratorConfiguration.ConfigurationsDirectory = configurationsDirectory;
            Configuration.ConfiguratorConfiguration.DeployerConfigurationFileName = deployerConfigurationFileName;

            string azureFunctionsConfigurationFilePath = Path.Combine(Configuration.ConfiguratorConfiguration.ConfigurationsDirectory, Configuration.ConfiguratorConfiguration.AzureFunctionsConfigurationBlobName);

            Logger.LogInformation(string.Format(Properties.Resources.AzureFunctionsConfigurationLoading, azureFunctionsConfigurationFilePath));

            string azureFunctionsConfigurationJson = await File.ReadAllTextAsync(azureFunctionsConfigurationFilePath);
            Configuration.AzureFunctionsConfiguration = JsonSerializer.Deserialize<AzureFunctionsConfiguration>(azureFunctionsConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;
            
            Logger.LogInformation(string.Format(Properties.Resources.ConfigurationLoaded));
        }
    }
}
