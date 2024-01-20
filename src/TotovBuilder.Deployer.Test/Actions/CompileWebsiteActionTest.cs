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
    /// Represents tests on the <see cref="CompileWebsiteAction"/> class.
    /// </summary>
    public class CompileWebsiteActionTest
    {
        [Fact]
        public void Caption_ShouldReturnCaption()
        {
            // Arrange
            CompileWebsiteAction action = new CompileWebsiteAction(
                new Mock<IApplicationLogger<CompileWebsiteAction>>().Object,
                new ApplicationConfiguration(),
                new Mock<IProcessWrapperFactory>().Object);

            // Act / Assert
            action.Caption.Should().Be(" 5 - Compile the website");
        }
    }
}
