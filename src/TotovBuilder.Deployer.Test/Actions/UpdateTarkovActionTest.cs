using FluentAssertions;
using Moq;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using Xunit;

namespace TotovBuilder.Deployer.Test.Actions
{
    /// <summary>
    /// Represents tests on the <see cref="UpdateTarkovAction"/> class.
    /// </summary>
    public class UpdateTarkovActionTest
    {
        [Fact]
        public void Caption_ShouldReturnCaption()
        {
            // Arrange
            UpdateTarkovAction action = new UpdateTarkovAction(
                new Mock<IApplicationLogger<UpdateTarkovAction>>().Object,
                new ApplicationConfiguration(),
                new Mock<IProcessWrapperFactory>().Object);

            // Act / Assert
            action.Caption.Should().Be(" 1 - Update Escape from Tarkov");
        }
    }
}
