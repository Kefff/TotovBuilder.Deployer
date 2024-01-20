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
                new Mock<IAzureBlobStorageManager>().Object);

            // Act / Assert
            action.Caption.Should().Be(" 8 - Deploy website to Azure");
        }
    }
}
