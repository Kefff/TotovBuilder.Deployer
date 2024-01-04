using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.Deployer.Test
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
            string extractionTestDirectory = Path.Combine(Path.GetTempPath(), "TotovBuilder.Deployer.Test");
            Directory.CreateDirectory(extractionTestDirectory);

            try
            {
                ConfigurationLoader configurationReader = new ConfigurationLoader();
                await configurationReader.WaitForLoading();
                configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory = extractionTestDirectory; // Changing the directory where data will be extracted after the configuration has been loaded

                File.WriteAllText(Path.Combine(extractionTestDirectory, configurationReader.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName), string.Empty);

                TarkovDataExtractor tarkovDataExtractor = new TarkovDataExtractor(configurationReader);

                // Act
                await tarkovDataExtractor.Extract();

                string itemsJson = await File.ReadAllTextAsync(
                    Path.Combine(
                        configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                        configurationReader.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName));
                ItemMissingProperties[] items = JsonSerializer.Deserialize<ItemMissingProperties[]>(itemsJson, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;

                // Assert
                items.Length.Should().BeGreaterThan(0);

                foreach (ItemMissingProperties expectedItemMissingProperties in TestData.ItemMissingProperties)
                {
                    ItemMissingProperties? item = items.SingleOrDefault(p => p.Id == expectedItemMissingProperties.Id);

                    if (item == null)
                    {
                        throw new Exception($"Cannot find missing properties for item \"{expectedItemMissingProperties.Id}\"");
                    }

                    item.Should().BeEquivalentTo(expectedItemMissingProperties);
                }

                string[] archivedFiles = Directory.GetFiles(
                    Path.Combine(
                        configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                        configurationReader.ConfiguratorConfiguration.PreviousExtractionsArchiveDirectory));
                archivedFiles.Any(f => f.EndsWith(configurationReader.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName)).Should().BeTrue();
            }
            finally
            {
                Directory.Delete(extractionTestDirectory, true);
            }
        }

        [Fact]
        public void Extract_WithInvalidTarkovResourceFileContent_ShouldThrow()
        {
            string extractionTestDirectory = Path.Combine(Path.GetTempPath(), "TotovBuilder.Deployer.Test");
            Directory.CreateDirectory(extractionTestDirectory);

            string tarkovResourcesFileName = "empty-resources.assets";
            string tarkovResourcesFilePath = Path.Combine(extractionTestDirectory, tarkovResourcesFileName);
            File.WriteAllText(tarkovResourcesFilePath, string.Empty);

            try
            {
                // Arrange
                DeployerConfiguration configuratorConfiguration = new DeployerConfiguration()
                {
                    TarkovResourcesFilePath = tarkovResourcesFilePath
                };

                Mock<IConfigurationLoader> configurationReaderMock = new Mock<IConfigurationLoader>();
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
