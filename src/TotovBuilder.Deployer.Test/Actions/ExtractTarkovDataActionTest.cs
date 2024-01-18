using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TotovBuilder.Deployer.Abstractions.Logs;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using TotovBuilder.Shared.Abstractions.Utils;
using Xunit;

namespace TotovBuilder.Deployer.Test.Actions
{
    /// <summary>
    /// Represents tests on the <see cref="ExtractTarkovDataAction"/> class.
    /// </summary>
    public class ExtractTarkovDataActionTest
    {
        [Fact]
        public async Task ExecuteAction_ShouldExtractItemMissingPropertiesAndArchiveOlderFile()
        {
            // Arrange
            string? extractionResultFileContent = null;

            ApplicationConfiguration configuration = new ApplicationConfiguration();
            configuration.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName = "item-missing-properties.json";
            configuration.DeployerConfiguration.ConfigurationsDirectory = "../../../../../../TotovBuilder.Configuration\\TEST";
            configuration.DeployerConfiguration.ItemsExtractionEndSearchString = "LocalProfile";
            configuration.DeployerConfiguration.ItemsExtractionStartSearchString = "TestItemTemplates";
            configuration.DeployerConfiguration.PreviousExtractionsArchiveDirectory = "archive";
            configuration.DeployerConfiguration.TarkovResourcesFilePath = "C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets";

            Mock<IFileWrapper> fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock
                .Setup(m => m.Exists("../../../../../../TotovBuilder.Configuration\\TEST\\item-missing-properties.json"))
                .Returns(true)
                .Verifiable();
            fileWrapperMock
                .Setup(m => m.Move("../../../../../../TotovBuilder.Configuration\\TEST\\item-missing-properties.json", $"../../../../../../TotovBuilder.Configuration\\TEST\\archive\\{DateTime.Now.ToString("yyyyMMddHHmmss_")}item-missing-properties.json"))
                .Verifiable();
            fileWrapperMock
                .Setup(m => m.WriteAllText("../../../../../../TotovBuilder.Configuration\\TEST\\item-missing-properties.json", It.IsAny<string>()))
                .Callback((string path, string contents) => extractionResultFileContent = contents)
                .Verifiable();

            Mock<IDirectoryWrapper> directoryWrapperMock = new Mock<IDirectoryWrapper>();
            directoryWrapperMock.Setup(m => m.CreateDirectory("../../../../../../TotovBuilder.Configuration\\TEST\\archive")).Verifiable();

            string[] lines = File.ReadAllLines("./TestData/resources.assets");
            int i = 0;

            Mock<IStreamReaderWrapper> streamReaderWrapperMock = new Mock<IStreamReaderWrapper>();
            streamReaderWrapperMock.Setup(m => m.ReadLine()).Returns(() =>
            {
                string? line = null;

                if (i < lines.Length)
                {
                    line = lines[i];
                    i++;
                }

                return line;
            }).Verifiable();

            Mock<IStreamReaderWrapperFactory> streamReaderWrapperFactoryMock = new Mock<IStreamReaderWrapperFactory>();
            streamReaderWrapperFactoryMock.Setup(m => m.Create("C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets")).Returns(streamReaderWrapperMock.Object);

            ExtractTarkovDataAction extractTarkovDataAction = new ExtractTarkovDataAction(
                new Mock<IApplicationLogger<ExtractTarkovDataAction>>().Object,
                configuration,
                fileWrapperMock.Object,
                directoryWrapperMock.Object,
                streamReaderWrapperFactoryMock.Object);

            // Act
            await extractTarkovDataAction.ExecuteAction();

            // Assert
            extractionResultFileContent.Should().NotBeNull();
            fileWrapperMock.Verify();
            directoryWrapperMock.Verify();
            streamReaderWrapperMock.Verify();

            foreach (ItemMissingProperties expectedItemMissingProperties in TestData.ItemMissingProperties)
            {
                bool hasBeenWritten = extractionResultFileContent!.Contains($"{{\"i\":\"{expectedItemMissingProperties.Id}\",\"a\":{expectedItemMissingProperties.MaxStackableAmount}}}");

                if (!hasBeenWritten)
                {
                    throw new Exception($"No match for item \"{expectedItemMissingProperties.Id}\" has been found.");
                }
            }

            // Clean
            Directory.Delete("./TestData", true);
        }

        //[Fact]
        //public void ExecuteAction_WithInvalidTarkovResourceFileContent_ShouldThrow()
        //{
        //        // Arrange
        //        DeployerConfiguration configuratorConfiguration = new DeployerConfiguration()
        //        {
        //            TarkovResourcesFilePath = tarkovResourcesFilePath
        //        };

        //        Mock<IConfigurationLoader> configurationReaderMock = new Mock<IConfigurationLoader>();
        //        configurationReaderMock.SetupGet(m => m.ConfiguratorConfiguration).Returns(configuratorConfiguration);

        //        TarkovDataExtractor tarkovDataExtractor = new TarkovDataExtractor(configurationReaderMock.Object);

        //        // Act
        //        Func<Task> act = () => tarkovDataExtractor.Extract();

        //        // Assert
        //        act.Should().ThrowAsync<Exception>("");
        //}
    }
}
