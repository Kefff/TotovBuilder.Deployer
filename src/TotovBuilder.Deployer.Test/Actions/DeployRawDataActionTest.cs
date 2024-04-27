using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Moq;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Deployer.Wrappers;
using TotovBuilder.Shared.Abstractions.Azure;
using Xunit;

namespace TotovBuilder.Deployer.Test.Actions
{
    /// <summary>
    /// Represents tests on the <see cref="DeployRawDataAction"/> class.
    /// </summary>
    public class DeployRawDataActionTest
    {
        [Fact]
        public void Caption_ShouldReturnCaption()
        {
            // Arrange
            DeployRawDataAction action = new DeployRawDataAction(
                new Mock<IApplicationLogger<DeployRawDataAction>>().Object,
                new ApplicationConfiguration(),
                new Mock<IFileWrapper>().Object,
                new Mock<IDirectoryWrapper>().Object,
                new Mock<IAzureBlobStorageManager>().Object);

            // Act / Assert
            action.Caption.Should().Be(" 6 - Deploy raw data to Azure");
        }

        [Fact]
        public async Task ExecuteAction_ShouldDeployRawDataToAzure()
        {
            // Arrange
            byte[] bytes = Encoding.UTF8.GetBytes("data");

            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            applicationConfiguration.DeployerConfiguration.ConfigurationsDirectory = "../TotovBuilder.Configuration";
            applicationConfiguration.AzureFunctionsConfiguration.RawItemCategoriesBlobName = "item-categories.json";
            applicationConfiguration.AzureFunctionsConfiguration.RawChangelogBlobName = "changelog.json";
            applicationConfiguration.AzureFunctionsConfiguration.AzureBlobStorageRawDataContainerName = "raw-data";

            Mock<IDirectoryWrapper> directoryWrapperMock = new Mock<IDirectoryWrapper>();
            directoryWrapperMock
                .Setup(m => m.GetFiles("../TotovBuilder.Configuration"))
                .Returns(new string[]
                    {
                        "../TotovBuilder.Configuration/changelog.json",
                        "../TotovBuilder.Configuration/item-categories.json",
                        "../TotovBuilder.Configuration/invalid.json"
                    })
                .Verifiable();

            Mock<IFileWrapper> fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(m => m.ReadAllBytes("../TotovBuilder.Configuration/changelog.json")).Returns(bytes).Verifiable();
            fileWrapperMock.Setup(m => m.ReadAllBytes("../TotovBuilder.Configuration/item-categories.json")).Returns(bytes).Verifiable();

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock.Setup(m => m.UpdateBlob("raw-data", "changelog.json", bytes, null)).Returns(Task.FromResult(Result.Ok())).Verifiable();
            azureBlobStorageManagerMock.Setup(m => m.UpdateBlob("raw-data", "item-categories.json", bytes, null)).Returns(Task.FromResult(Result.Ok())).Verifiable();

            DeployRawDataAction action = new DeployRawDataAction(
                new Mock<IApplicationLogger<DeployRawDataAction>>().Object,
                applicationConfiguration,
                fileWrapperMock.Object,
                directoryWrapperMock.Object,
                azureBlobStorageManagerMock.Object);

            // Act
            await action.ExecuteAction();

            // Assert
            directoryWrapperMock.Verify();
            fileWrapperMock.Verify();
            azureBlobStorageManagerMock.Verify();
        }
    }
}
