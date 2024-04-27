using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Deployer.Wrappers;
using TotovBuilder.Model.Configuration;
using Xunit;
using Xunit.Sdk;

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

        [Fact]
        public async Task ExecuteAction_ShouldCompileWebsite()
        {
            // Arrange
            bool hasReadenLine = false;

            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            applicationConfiguration.DeployerConfiguration.WebsiteCompilationCommand = "npm build-test";
            applicationConfiguration.DeployerConfiguration.WebsiteDirectoryPath = "C:\\website";

            ProcessStartInfo startInfo = new ProcessStartInfo();

            Mock<IStreamReaderWrapper> streamReaderWrapperMock = new Mock<IStreamReaderWrapper>();
            streamReaderWrapperMock
                .Setup(m => m.ReadLine())
                .Returns(() =>
                {
                    if (!hasReadenLine)
                    {
                        hasReadenLine = true;
                        return "Website compiled";
                    }
                    
                    return null;
                })
                .Verifiable();

            Mock<IStreamWriterWrapper> streamWriterWrapperMock = new Mock<IStreamWriterWrapper>();
            streamWriterWrapperMock.Setup(m => m.WriteLine("npm build-test & exit")).Verifiable();

            Mock<IProcessWrapper> processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.SetupGet(m => m.StandardInput).Returns(streamWriterWrapperMock.Object).Verifiable();
            processWrapperMock.SetupGet(m => m.StandardOutput).Returns(streamReaderWrapperMock.Object).Verifiable();
            processWrapperMock.SetupGet(m => m.StartInfo).Returns(startInfo).Verifiable();
            processWrapperMock.Setup(m => m.Start()).Verifiable();
            processWrapperMock.Setup(m => m.WaitForExit()).Verifiable();

            Mock<IProcessWrapperFactory> processWrapperFactory = new Mock<IProcessWrapperFactory>();
            processWrapperFactory.Setup(m => m.Create()).Returns(processWrapperMock.Object);

            CompileWebsiteAction action = new CompileWebsiteAction(
                new Mock<IApplicationLogger<CompileWebsiteAction>>().Object,
                applicationConfiguration,
                processWrapperFactory.Object);

            // Act
            await action.ExecuteAction();

            // Assert
            startInfo.FileName.Should().Be("cmd");
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WorkingDirectory = "C:\\website";
            streamReaderWrapperMock.Verify();
            streamWriterWrapperMock.Verify();
            processWrapperMock.Verify();
            processWrapperFactory.Verify();
        }
    }
}
