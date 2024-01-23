using System.Diagnostics;
using System.Threading.Tasks;
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

        [Fact]
        public async Task ExecuteAction_ShouldStartTarkovLauncher()
        {
            // Arrange
            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            applicationConfiguration.DeployerConfiguration.TarkovLauncherExecutableFilePath = "C:/Battlestate Games/BsgLauncher/BsgLauncher.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo();

            Mock<IProcessWrapper> processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.SetupGet(m => m.StartInfo).Returns(startInfo).Verifiable();
            processWrapperMock.Setup(m => m.Start()).Verifiable();

            Mock<IProcessWrapperFactory> processWrapperFactory = new Mock<IProcessWrapperFactory>();
            processWrapperFactory.Setup(m => m.Create()).Returns(processWrapperMock.Object);

            UpdateTarkovAction action = new UpdateTarkovAction(
                new Mock<IApplicationLogger<UpdateTarkovAction>>().Object,
                applicationConfiguration,
                processWrapperFactory.Object);

            // Act
            await action.ExecuteAction();

            // Assert
            startInfo.FileName.Should().Be("C:/Battlestate Games/BsgLauncher/BsgLauncher.exe");
            startInfo.CreateNoWindow = true;
            processWrapperMock.Verify();
            processWrapperFactory.Verify();
        }
    }
}
