using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TotovBuilder.Configurator.Abstractions;
using Xunit;

namespace TotovBuilder.Configurator.Test
{
    /// <summary>
    /// Represents tests on the <see cref="ConfigurationReader"/> class.
    /// </summary>
    public class ConfigurationReaderTest
    {
        [Fact]
        public async Task WaitForLoading_ShouldWaitForConfigurationToBeLoaded()
        {
            // Arrange
            ConfigurationReader configurationReader = new ConfigurationReader();

            // Act
            await configurationReader.WaitForLoading();

            // Assert
            configurationReader.AzureFunctionsConfiguration.AzureBlobStorageConnectionString.Should().NotBeEmpty();
            configurationReader.AzureFunctionsConfiguration.AzureBlobStorageContainerName.Should().NotBeEmpty();
            configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory.Should().NotBeEmpty();
            configurationReader.ConfiguratorConfiguration.ConfiguratorConfigurationFileName.Should().NotBeEmpty();
        }
    }
}
