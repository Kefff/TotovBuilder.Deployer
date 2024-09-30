using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Model;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.Deployer.Test.Configuration
{
    /// <summary>
    /// Represents tests on the <see cref="ConfigurationLoader"/> class.
    /// </summary>
    public class ConfigurationLoaderTest
    {
        [Theory]
        [InlineData(DeploymentMode.Production)]
        [InlineData(DeploymentMode.Test)]
        public async Task Load_ShouldLoadConfiguration(DeploymentMode deploymentMode)
        {
            // Arrange
            Mock<IFileWrapper> fileWrapperMock = new();
            fileWrapperMock
                .Setup(m => m.ReadAllTextAsync($"../../../../../../TotovBuilder.Configuration\\{deploymentMode.ToString().ToUpperInvariant()}\\deployer-configuration.json"))
                .Returns(Task.FromResult(@"{
  ""AzureFunctionsConfigurationBlobName"": ""azure-functions-configuration.json"",
  ""ItemsExtractionEndSearchString"": ""LocalProfile"",
  ""ItemsExtractionStartSearchString"": ""TestItemTemplates"",
  ""PreviousExtractionsArchiveDirectory"": ""archive"",
  ""TarkovLauncherExecutableFilePath"": ""C:/Battlestate Games/BsgLauncher/BsgLauncher.exe"",
  ""TarkovResourcesFilePath"": ""C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets"",
  ""WebsiteBuildDirectory"": ""dist"",
  ""WebsiteCompilationCommand"": ""npm run build-test"",
  ""WebsiteDeploymentFileNotToDeletePattern"": ""data/.*"",
  ""WebsiteDirectoryPath"": ""D:/TotovBuilder/TotovBuilder.Website""
}"))
                .Verifiable();
            fileWrapperMock
                .Setup(m => m.ReadAllTextAsync($"../../../../../../TotovBuilder.Configuration\\{deploymentMode.ToString().ToUpperInvariant()}\\azure-functions-configuration.json"))
                .Returns(Task.FromResult(TestData.AzureFunctionsConfigurationJson))
                .Verifiable();

            ApplicationConfiguration configuration = new();

            ConfigurationLoader configurationReader = new(
                new Mock<IApplicationLogger<ConfigurationLoader>>().Object,
                configuration,
                fileWrapperMock.Object);

            // Act
            await configurationReader.Load(deploymentMode);

            // Assert
            configuration.AzureFunctionsConfiguration.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
            configuration.DeployerConfiguration.Should().BeEquivalentTo(new DeployerConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json",
                ConfigurationsDirectory = $"../../../../../../TotovBuilder.Configuration\\{deploymentMode.ToString().ToUpperInvariant()}",
                DeployerConfigurationFileName = "deployer-configuration.json",
                DeployerDeploymentMode = deploymentMode,
                ItemsExtractionEndSearchString = "LocalProfile",
                ItemsExtractionStartSearchString = "TestItemTemplates",
                PreviousExtractionsArchiveDirectory = "archive",
                TarkovLauncherExecutableFilePath = "C:/Battlestate Games/BsgLauncher/BsgLauncher.exe",
                TarkovResourcesFilePath = "C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets",
                WebsiteBuildDirectory = "dist",
                WebsiteCompilationCommand = "npm run build-test",
                WebsiteDeploymentFileNotToDeletePattern = "data/.*",
                WebsiteDirectoryPath = "D:/TotovBuilder/TotovBuilder.Website"
            });
        }
    }
}
