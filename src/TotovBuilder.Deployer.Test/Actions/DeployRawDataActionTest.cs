using FluentAssertions;
using Moq;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
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
                new Mock<IAzureBlobStorageManager>().Object);

            // Act / Assert
            action.Caption.Should().Be(" 6 - Deploy raw data to Azure");
        }
    }
}
