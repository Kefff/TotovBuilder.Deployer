using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.Deployer.Test.Actions
{
    /// <summary>
    /// Represents tests on the <see cref="ExtractTarkovDataAction"/> class.
    /// </summary>
    public class ExtractTarkovDataActionTest
    {
        [Fact]
        public void Caption_ShouldReturnCaption()
        {
            // Arrange
            ExtractTarkovDataAction action = new(
                new Mock<IApplicationLogger<ExtractTarkovDataAction>>().Object,
                new ApplicationConfiguration(),
                new Mock<IFileWrapper>().Object,
                new Mock<IDirectoryWrapper>().Object,
                new Mock<IStreamReaderWrapperFactory>().Object);

            // Act / Assert
            action.Caption.Should().Be(" 2 - Extract missing item properties from Escape from Tarkov");
        }

        [Fact]
        public async Task ExecuteAction_ShouldExtractItemMissingPropertiesAndArchiveOlderFile()
        {
            // Arrange
            string? extractionResultFileContent = null;

            ApplicationConfiguration configuration = new();
            configuration.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName = "item-missing-properties.json";
            configuration.DeployerConfiguration.ConfigurationsDirectory = "../../../../../../TotovBuilder.Configuration\\TEST";
            configuration.DeployerConfiguration.ItemsExtractionEndSearchString = "LocalProfile";
            configuration.DeployerConfiguration.ItemsExtractionStartSearchString = "TestItemTemplates";
            configuration.DeployerConfiguration.PreviousExtractionsArchiveDirectory = "archive";
            configuration.DeployerConfiguration.TarkovResourcesFilePath = "C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets";

            Mock<IFileWrapper> fileWrapperMock = new();
            fileWrapperMock
                .Setup(m => m.Exists("../../../../../../TotovBuilder.Configuration\\TEST\\item-missing-properties.json"))
                .Returns(true)
                .Verifiable();
            fileWrapperMock
                .Setup(m => m.Move("../../../../../../TotovBuilder.Configuration\\TEST\\item-missing-properties.json", $"../../../../../../TotovBuilder.Configuration\\TEST\\archive\\{DateTime.Now:yyyyMMddHHmmss_}item-missing-properties.json"))
                .Verifiable();
            fileWrapperMock
                .Setup(m => m.WriteAllText("../../../../../../TotovBuilder.Configuration\\TEST\\item-missing-properties.json", It.IsAny<string>()))
                .Callback((string path, string contents) => extractionResultFileContent = contents)
                .Verifiable();

            Mock<IDirectoryWrapper> directoryWrapperMock = new();
            directoryWrapperMock.Setup(m => m.CreateDirectory("../../../../../../TotovBuilder.Configuration\\TEST\\archive")).Verifiable();

            string[] lines = File.ReadAllLines("./TestData/resources.assets");
            int i = 0;

            Mock<IStreamReaderWrapper> streamReaderWrapperMock = new();
            streamReaderWrapperMock
                .Setup(m => m.ReadLine())
                .Returns(() =>
                {
                    string? line = null;

                    if (i < lines.Length)
                    {
                        line = lines[i];
                        i++;
                    }

                    return line;
                })
                .Verifiable();

            Mock<IStreamReaderWrapperFactory> streamReaderWrapperFactoryMock = new();
            streamReaderWrapperFactoryMock.Setup(m => m.Create("C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets")).Returns(streamReaderWrapperMock.Object);

            ExtractTarkovDataAction action = new(
                new Mock<IApplicationLogger<ExtractTarkovDataAction>>().Object,
                configuration,
                fileWrapperMock.Object,
                directoryWrapperMock.Object,
                streamReaderWrapperFactoryMock.Object);

            // Act
            await action.ExecuteAction();

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
        }

        [Fact]
        public async Task ExecuteAction_WithNoTarkovResources_ShouldLogError()
        {
            // Arrange
            ApplicationConfiguration configuration = new();
            configuration.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName = "item-missing-properties.json";
            configuration.DeployerConfiguration.ConfigurationsDirectory = "../../../../../../TotovBuilder.Configuration\\TEST";
            configuration.DeployerConfiguration.ItemsExtractionEndSearchString = "LocalProfile";
            configuration.DeployerConfiguration.ItemsExtractionStartSearchString = "TestItemTemplates";
            configuration.DeployerConfiguration.PreviousExtractionsArchiveDirectory = "archive";
            configuration.DeployerConfiguration.TarkovResourcesFilePath = "C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets";

            Mock<IApplicationLogger<ExtractTarkovDataAction>> loggerMock = new();
            loggerMock.Setup(m => m.LogError("Cannot read Escape from Tarkov resources file content.")).Verifiable();

            string[] lines =
            [
                "LocalProfile",
                "\"data\": {",
                "{",
                "}",
                "}",
                "TestItemTemplates"
            ];
            int i = 0;

            Mock<IStreamReaderWrapper> streamReaderWrapperMock = new();
            streamReaderWrapperMock
                .Setup(m => m.ReadLine())
                .Returns(() =>
                {
                    string? line = null;

                    if (i < lines.Length)
                    {
                        line = lines[i];
                        i++;
                    }

                    return line;
                })
                .Verifiable();

            Mock<IStreamReaderWrapperFactory> streamReaderWrapperFactoryMock = new();
            streamReaderWrapperFactoryMock.Setup(m => m.Create("C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets")).Returns(streamReaderWrapperMock.Object);

            ExtractTarkovDataAction action = new(
                loggerMock.Object,
                configuration,
                new Mock<IFileWrapper>().Object,
                new Mock<IDirectoryWrapper>().Object,
                streamReaderWrapperFactoryMock.Object);

            // Act
            await action.ExecuteAction();

            // Assert
            loggerMock.Verify();
            streamReaderWrapperMock.Verify();
        }
    }
}
