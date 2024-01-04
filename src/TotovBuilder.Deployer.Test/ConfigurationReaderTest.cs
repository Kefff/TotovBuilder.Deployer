using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TotovBuilder.Deployer.Test
{
    /// <summary>
    /// Represents tests on the <see cref="ConfigurationLoader"/> class.
    /// </summary>
    public class ConfigurationReaderTest
    {
        [Fact]
        public async Task WaitForLoading_ShouldWaitForConfigurationToBeLoaded()
        {
            // Arrange
            ConfigurationLoader configurationReader = new ConfigurationLoader();

            // Act
            await configurationReader.WaitForLoading();

            // Assert
            configurationReader.AzureFunctionsConfiguration.AzureBlobStorageConnectionString.Should().NotBeEmpty();
            configurationReader.AzureFunctionsConfiguration.AzureBlobStorageRawDataContainerName.Should().NotBeEmpty();
            configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory.Should().NotBeEmpty();
            configurationReader.ConfiguratorConfiguration.DeployerConfigurationFileName.Should().NotBeEmpty();
        }
    }
}
