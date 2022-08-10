using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TotovBuilder.Configurator.Abstractions;
using TotovBuilder.Model;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
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

                File.WriteAllText(Path.Combine(extractionTestDirectory, configurationReader.AzureFunctionsConfiguration.AzureItemMissingPropertiesBlobName), string.Empty);

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

                foreach (ItemMissingProperties expectedItemMissingProperties in TestData.ItemMissingProperties)
                {
                    ItemMissingProperties item = items.Single(p => p.Id == expectedItemMissingProperties.Id);
                    item.Should().BeEquivalentTo(expectedItemMissingProperties);
                }

                string[] archivedFiles = Directory.GetFiles(
                    Path.Combine(
                        configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                        configurationReader.ConfiguratorConfiguration.PreviousExtractionsArchiveDirectory));
                archivedFiles.Any(f => f.EndsWith(configurationReader.AzureFunctionsConfiguration.AzureItemMissingPropertiesBlobName)).Should().BeTrue();
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
                
                File.WriteAllText(Path.Combine(extractionTestDirectory, configurationReader.AzureFunctionsConfiguration.AzurePresetsBlobName), string.Empty);

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

                foreach (InventoryItem expectedPreset in TestData.Presets)
                {
                    InventoryItem preset = presets.Single(p => p.ItemId == expectedPreset.ItemId);
                    preset.Should().BeEquivalentTo(expectedPreset);
                }

                string[] archivedFiles = Directory.GetFiles(
                    Path.Combine(
                        configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                        configurationReader.ConfiguratorConfiguration.PreviousExtractionsArchiveDirectory));
                archivedFiles.Any(f => f.EndsWith(configurationReader.AzureFunctionsConfiguration.AzurePresetsBlobName)).Should().BeTrue();
            }
            finally
            {
                Directory.Delete(extractionTestDirectory, true);
            }
        }

        [Fact]
        public void Extract_WithInvalidTarkovResourceFileContent_ShouldThrow()
        {
            string extractionTestDirectory = Path.Combine(Path.GetTempPath(), "TotovBuilder.Configurator.Test");
            Directory.CreateDirectory(extractionTestDirectory);

            string tarkovResourcesFileName = "empty-resources.assets";
            string tarkovResourcesFilePath = Path.Combine(extractionTestDirectory, tarkovResourcesFileName);
            File.WriteAllText(tarkovResourcesFilePath, string.Empty);

            try
            {
                // Arrange
                ConfiguratorConfiguration configuratorConfiguration = new ConfiguratorConfiguration()
                {
                    TarkovResourcesFilePath = tarkovResourcesFilePath
                };

                Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
                configurationReaderMock.SetupGet(m => m.ConfiguratorConfiguration).Returns(configuratorConfiguration);

                TarkovDataExtractor tarkovDataExtractor = new TarkovDataExtractor(configurationReaderMock.Object);

                // Act
                Func<Task> act = () => tarkovDataExtractor.Extract();

                // Assert
                act.Should().ThrowAsync<Exception>("");
            }
            finally
            {
                Directory.Delete(extractionTestDirectory, true);
            }
        }
    }
}
