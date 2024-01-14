using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Logs;
using TotovBuilder.Model;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Shared.Abstractions.Utils;

namespace TotovBuilder.Deployer.Configuration
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
        /// File wrapper.
        /// </summary>
        private readonly IFileWrapper FileWrapper;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<ConfigurationLoader> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationLoader"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="fileWrapper">File wrapper.</param>
        public ConfigurationLoader(IApplicationLogger<ConfigurationLoader> logger, IApplicationConfiguration configuration, IFileWrapper fileWrapper)
        {
            Configuration = configuration;
            FileWrapper = fileWrapper;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task Load(DeploymentMode deploymentMode)
        {
            string configurationsDirectory = Path.Combine(ConfigurationManager.AppSettings.Get(ConfigurationsDirectoryKey)!, deploymentMode.ToString().ToUpper());
            string deployerConfigurationFileName = ConfigurationManager.AppSettings.Get(DeployerConfigurationFileNameKey)!;
            string deployerConfigurationFilePath = Path.Combine(configurationsDirectory, deployerConfigurationFileName);

            Logger.LogInformation(string.Format(Properties.Resources.LoadingDeployerConfiguration, deployerConfigurationFilePath));

            string deployerConfigurationJson = await FileWrapper.ReadAllTextAsync(deployerConfigurationFilePath);
            Configuration.DeployerConfiguration = JsonSerializer.Deserialize<DeployerConfiguration>(deployerConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;
            Configuration.DeployerConfiguration.DeployerDeploymentMode = deploymentMode;
            Configuration.DeployerConfiguration.ConfigurationsDirectory = configurationsDirectory;
            Configuration.DeployerConfiguration.DeployerConfigurationFileName = deployerConfigurationFileName;

            string azureFunctionsConfigurationFilePath = Path.Combine(Configuration.DeployerConfiguration.ConfigurationsDirectory, Configuration.DeployerConfiguration.AzureFunctionsConfigurationBlobName);

            Logger.LogInformation(string.Format(Properties.Resources.LoadingAzureFunctionsConfiguration, azureFunctionsConfigurationFilePath));

            string azureFunctionsConfigurationJson = await FileWrapper.ReadAllTextAsync(azureFunctionsConfigurationFilePath);
            Configuration.AzureFunctionsConfiguration = JsonSerializer.Deserialize<AzureFunctionsConfiguration>(azureFunctionsConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;

            Logger.LogSuccess(string.Format(Properties.Resources.ConfigurationLoaded));
        }
    }
}
