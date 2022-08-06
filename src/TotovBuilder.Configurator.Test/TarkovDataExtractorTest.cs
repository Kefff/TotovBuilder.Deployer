using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using TotovBuilder.Model;
using TotovBuilder.Model.Builds;
using Xunit;

namespace TotovBuilder.Configurator.Test
{
    /// <summary>
    /// Represents tests on the <see cref="TarkovDataExtractor"/> class.
    /// </summary>
    public class TarkovDataExtractorTest
    {
        [Fact]
        public async Task Extract_ShouldExtractItems()
        {
            // Arrange
            string extractionTestDirectory = Path.Combine(Path.GetTempPath(), "TotovBuilder.Configurator.Test");
            Directory.CreateDirectory(extractionTestDirectory);

            try
            {
                ConfigurationReader configurationReader = new ConfigurationReader();
                await configurationReader.WaitForLoading();
                configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory = extractionTestDirectory; // Changing the directory where items and presets will be extracted after the configuration has been loaded

                TarkovDataExtractor tarkovDataExtractor = new TarkovDataExtractor(configurationReader);

                // Act
                await tarkovDataExtractor.Extract();

                string itemsJson = await File.ReadAllTextAsync(
                    Path.Combine(
                        configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                        configurationReader.AzureFunctionsConfiguration.AzureItemMissingPropertiesBlobName));
                ItemMissingProperties[] items = JsonSerializer.Deserialize<ItemMissingProperties[]>(itemsJson, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });

                // Assert
                items.Length.Should().BeGreaterThan(0);
            }
            finally
            {
                Directory.Delete(extractionTestDirectory, true);
            }
        }
        
        [Fact]
        public async Task Extract_ShouldExtractPresets()
        {
            // Arrange
            string extractionTestDirectory = Path.Combine(Path.GetTempPath(), "TotovBuilder.Configurator.Test");
            Directory.CreateDirectory(extractionTestDirectory);

            try
            {
                ConfigurationReader configurationReader = new ConfigurationReader();
                await configurationReader.WaitForLoading();
                configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory = extractionTestDirectory; // Changing the directory where items and presets will be extracted after the configuration has been loaded

                TarkovDataExtractor tarkovDataExtractor = new TarkovDataExtractor(configurationReader);

                // Act
                await tarkovDataExtractor.Extract();

                string itemsJson = await File.ReadAllTextAsync(
                    Path.Combine(
                        configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                        configurationReader.AzureFunctionsConfiguration.AzurePresetsBlobName));
                InventoryItem[] presets = JsonSerializer.Deserialize<InventoryItem[]>(itemsJson, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });

                // Assert
                presets.Length.Should().BeGreaterThan(0);
            }
            finally
            {
                Directory.Delete(extractionTestDirectory, true);
            }
        }
    }
}
