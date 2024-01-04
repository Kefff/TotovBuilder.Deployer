using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TotovBuilder.Deployer.Test
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
            configurationReader.AzureFunctionsConfiguration.AzureBlobStorageRawDataContainerName.Should().NotBeEmpty();
            configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory.Should().NotBeEmpty();
            configurationReader.ConfiguratorConfiguration.ConfiguratorConfigurationFileName.Should().NotBeEmpty();
        }
    }
}
