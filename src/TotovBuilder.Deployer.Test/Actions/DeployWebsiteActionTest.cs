using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using FluentResults;
using Moq;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Shared.Abstractions.Azure;
using Xunit;

namespace TotovBuilder.Deployer.Test.Actions
{
    /// <summary>
    /// Represents tests on the <see cref="DeployWebsiteAction"/> class.
    /// </summary>
    public class DeployWebsiteActionTest
    {
        [Fact]
        public void Caption_ShouldReturnCaption()
        {
            // Arrange
            DeployWebsiteAction action = new DeployWebsiteAction(
                new Mock<IApplicationLogger<DeployWebsiteAction>>().Object,
                new ApplicationConfiguration(),
                new Mock<IFileWrapper>().Object,
                new Mock<IDirectoryWrapper>().Object,
                new Mock<IAzureBlobStorageManager>().Object);

            // Act / Assert
            action.Caption.Should().Be(" 8 - Deploy website to Azure");
        }

        [Fact]
        public async Task ExecuteAction_ShouldDeployWebsiteToAzure()
        {
            // Arrange
            byte[] bytes = Encoding.UTF8.GetBytes("data");
            BlobHttpHeaders? createdBlobHttpHeaders = null;

            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            applicationConfiguration.AzureFunctionsConfiguration.AzureBlobStorageWebsiteContainerName = "$web";
            applicationConfiguration.AzureFunctionsConfiguration.WebsiteFileCacheControl = "max-age=31536000, must-revalidate";
            applicationConfiguration.DeployerConfiguration.WebsiteBuildDirectory = "dist";
            applicationConfiguration.DeployerConfiguration.WebsiteDeploymentFileNotToDeletePattern = "data/.*";
            applicationConfiguration.DeployerConfiguration.WebsiteDirectoryPath = "C:/TotovBuilder.Website";

            Mock<IDirectoryWrapper> directoryWrapperMock = new Mock<IDirectoryWrapper>();
            directoryWrapperMock.Setup(m => m.GetDirectories("C:/TotovBuilder.Website\\dist"))
                .Returns(new string[]
                    {
                        "C:/TotovBuilder.Website\\dist/fonts",
                        "C:/TotovBuilder.Website\\dist/images"
                    });
            directoryWrapperMock
                .Setup(m => m.GetFiles("C:/TotovBuilder.Website\\dist"))
                .Returns(new string[] { "C:/TotovBuilder.Website\\dist/index.html" })
                .Verifiable();
            directoryWrapperMock
                .Setup(m => m.GetFiles("C:/TotovBuilder.Website\\dist/fonts"))
                .Returns(new string[] { "C:/TotovBuilder.Website\\dist/fonts/escape-from-tarkov.ttf" })
                .Verifiable();
            directoryWrapperMock
                .Setup(m => m.GetFiles("C:/TotovBuilder.Website\\dist/images"))
                .Returns(new string[] { "C:/TotovBuilder.Website\\dist/images/prapor.webp" })
                .Verifiable();

            Mock<IFileWrapper> fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(m => m.ReadAllBytes("C:/TotovBuilder.Website\\dist/index.html")).Returns(bytes).Verifiable();
            fileWrapperMock.Setup(m => m.ReadAllBytes("C:/TotovBuilder.Website\\dist/fonts/escape-from-tarkov.ttf")).Returns(bytes).Verifiable();
            fileWrapperMock.Setup(m => m.ReadAllBytes("C:/TotovBuilder.Website\\dist/images/prapor.webp")).Returns(bytes).Verifiable();

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateContainer(
                    "$web",
                    new Dictionary<string, byte[]>()
                    {
                        { "C:/TotovBuilder.Website\\dist/index.html", bytes },
                        { "C:/TotovBuilder.Website\\dist/fonts/escape-from-tarkov.ttf", bytes },
                        { "C:/TotovBuilder.Website\\dist/images/prapor.webp", bytes }
                    },
                    It.IsAny<Func<BlobHttpHeaders>>(),
                    "data/.*"))
                .Callback((string containerName, Dictionary<string, byte[]> data, Func<BlobHttpHeaders> createHttpHeadersFunction, string[] deletionIgnorePatterns) => createdBlobHttpHeaders = createHttpHeadersFunction())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            DeployWebsiteAction action = new DeployWebsiteAction(
                new Mock<IApplicationLogger<DeployWebsiteAction>>().Object,
                applicationConfiguration,
                fileWrapperMock.Object,
                directoryWrapperMock.Object,
                azureBlobStorageManagerMock.Object);

            // Act
            await action.ExecuteAction();

            // Assert
            createdBlobHttpHeaders.Should().NotBeNull();
            createdBlobHttpHeaders!.CacheControl.Should().Be("max-age=31536000, must-revalidate");
            directoryWrapperMock.Verify();
            fileWrapperMock.Verify();
            azureBlobStorageManagerMock.Verify();
        }
    }
}
