using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TotovBuilder.Configurator.Abstractions;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Configurator
{
    /// <summary>
    /// Represents a configuration reader.
    /// </summary>
    public class ConfigurationReader : IConfigurationReader
    {
        private const string ConfigurationsDirectoryKey = "ConfigurationsDirectory";
        private const string ConfiguratorConfigurationFileNameKey = "ConfiguratorConfigurationFileName";

        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        public AzureFunctionsConfiguration AzureFunctionsConfiguration { get; private set; } = new AzureFunctionsConfiguration();

        /// <summary>
        /// Configurator configuration.
        /// </summary>
        public ConfiguratorConfiguration ConfiguratorConfiguration { get; private set; } = new ConfiguratorConfiguration();

        /// <summary>
        /// Loading task.
        /// </summary>
        private readonly Task LoadingTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationReader"/> class.
        /// </summary>
        public ConfigurationReader()
        {
            LoadingTask = Load();
        }

        /// <summary>
        /// Waits for the configuration to be loaded.
        /// </summary>
        public Task WaitForLoading()
        {
            return LoadingTask;
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        private async Task Load()
        {
            Logger.LogInformation(string.Format(Properties.Resources.ReadingConfiguratorConfiguration));

            string configurationsDirectory = ConfigurationManager.AppSettings.Get(ConfigurationsDirectoryKey);
            string configuratorConfigurationFileName = ConfigurationManager.AppSettings.Get(ConfiguratorConfigurationFileNameKey);
            
            string configuratorConfigurationJson = await File.ReadAllTextAsync(Path.Combine(configurationsDirectory, configuratorConfigurationFileName));
            ConfiguratorConfiguration = JsonSerializer.Deserialize<ConfiguratorConfiguration>(configuratorConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            ConfiguratorConfiguration.ConfigurationsDirectory = configurationsDirectory;
            ConfiguratorConfiguration.ConfiguratorConfigurationFileName = configuratorConfigurationFileName;

            Logger.LogInformation(string.Format(Properties.Resources.ReadingAzureFunctionsConfiguration));

            string azureFunctionsConfigurationJson = await File.ReadAllTextAsync(Path.Combine(ConfiguratorConfiguration.ConfigurationsDirectory, ConfiguratorConfiguration.AzureFunctionsConfigurationBlobName));
            AzureFunctionsConfiguration = JsonSerializer.Deserialize<AzureFunctionsConfiguration>(azureFunctionsConfigurationJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            Logger.LogInformation(string.Format(Properties.Resources.ReadingNonStandardPresetNames));
            string nonStandardPresetNamesJson = await File.ReadAllTextAsync(Path.Combine(ConfiguratorConfiguration.ConfigurationsDirectory, ConfiguratorConfiguration.NonStandardPresetNamesFileName));
            ConfiguratorConfiguration.NonStandardPresetNames = JsonSerializer.Deserialize<string[]>(nonStandardPresetNamesJson);
        }
    }
}
