using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Logs;
using TotovBuilder.Deployer.Actions;
using TotovBuilder.Deployer.Configuration;
using TotovBuilder.Model;
using TotovBuilder.Model.Configuration;
using Xunit;

namespace TotovBuilder.Deployer.Test
{
    /// <summary>
    /// Represents tests on the <see cref="Deployer"/> class.
    /// </summary>
    public class DeployerTest
    {
        [Fact]
        public async Task Run_WithTestDeploymentMode_ShouldLoadConfiguration()
        {
            // Arrange
            IApplicationConfiguration configuration = new ApplicationConfiguration();

            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            Mock<IPromtWrapper> promptWrapperMock = new Mock<IPromtWrapper>();
            promptWrapperMock
                .Setup(m => m.Select(
                    "Deployment mode",
                    It.IsAny<DeploymentMode[]>()))
                .Returns(DeploymentMode.Test);
            promptWrapperMock
                .Setup(m => m.Select(
                    "Select an action",
                    It.IsAny<IEnumerable<string>>()))
                .Returns("     Exit");

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();
            configurationLoaderMock
                .Setup(m => m.Load(DeploymentMode.Test))
                .Callback(() => configuration.DeployerConfiguration = new DeployerConfiguration()
                {
                    AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json",
                    ConfigurationsDirectory = $"../../../../../../TotovBuilder.Configuration\\{DeploymentMode.Test.ToString().ToUpperInvariant()}",
                    DeployerConfigurationFileName = "deployer-configuration.json",
                    DeployerDeploymentMode = DeploymentMode.Test,
                    ItemsExtractionEndSearchString = "LocalProfile",
                    ItemsExtractionStartSearchString = "TestItemTemplates",
                    PreviousExtractionsArchiveDirectory = "archive",
                    TarkovLauncherExecutableFilePath = "C:/Battlestate Games/BsgLauncher/BsgLauncher.exe",
                    TarkovResourcesFilePath = "C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets",
                    WebsiteBuildDirectory = "dist",
                    WebsiteCompilationCommand = "npm run build-test",
                    WebsiteDeploymentFileNotToDeletePattern = "data/.*",
                    WebsiteDirectoryPath = "D:/TotovBuilder/TotovBuilder.Website"
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            Deployer deployer = new Deployer(
                new Mock<IApplicationLogger<Deployer>>().Object,
                consoleWrapperMock.Object,
                promptWrapperMock.Object,
                configurationLoaderMock.Object,
                configuration,
                new Mock<IDeploymentAction<CompileWebsiteAction>>().Object,
                new Mock<IDeploymentAction<DeployRawDataAction>>().Object,
                new Mock<IDeploymentAction<DeployWebsiteAction>>().Object,
                new Mock<IDeploymentAction<ExtractTarkovDataAction>>().Object,
                new Mock<IDeploymentAction<UpdateTarkovAction>>().Object);

            // Act
            await deployer.Run();

            // Assert
            configurationLoaderMock.Verify();
            consoleWrapperMock.Verify(m => m.WriteLine(DeploymentMode.Test.ToString()!.ToUpperInvariant()));
        }

        [Theory]
        [InlineData("YES")]
        [InlineData("yes")]
        [InlineData("Yes")]
        public async Task Run_WithProductionDeploymentMode_ShouldLoadConfiguration(string confirmationText)
        {
            // Arrange
            bool hasFailedConfirmation = false;
            IApplicationConfiguration configuration = new ApplicationConfiguration();

            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            Mock<IPromtWrapper> promptWrapperMock = new Mock<IPromtWrapper>();
            promptWrapperMock
                .Setup(m => m.Select(
                    "Deployment mode",
                    It.IsAny<DeploymentMode[]>()))
                .Returns(DeploymentMode.Production);
            promptWrapperMock
                .Setup(m => m.Input<string>("Are you sure you want to use deployment mode \"PRODUCTION\"? Confirm by typing \"Yes\""))
                .Callback(() =>
                {
                    if (!hasFailedConfirmation)
                    {
                        hasFailedConfirmation = true;
                    }
                })
                .Returns(() => hasFailedConfirmation ? confirmationText : "No");
            promptWrapperMock
                .Setup(m => m.Select(
                    "Select an action",
                    It.IsAny<IEnumerable<string>>()))
                .Returns("     Exit");

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();
            configurationLoaderMock
                .Setup(m => m.Load(DeploymentMode.Production))
                .Callback(() => configuration.DeployerConfiguration = new DeployerConfiguration()
                {
                    AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json",
                    ConfigurationsDirectory = $"../../../../../../TotovBuilder.Configuration\\{DeploymentMode.Production.ToString().ToUpperInvariant()}",
                    DeployerConfigurationFileName = "deployer-configuration.json",
                    DeployerDeploymentMode = DeploymentMode.Production,
                    ItemsExtractionEndSearchString = "LocalProfile",
                    ItemsExtractionStartSearchString = "TestItemTemplates",
                    PreviousExtractionsArchiveDirectory = "archive",
                    TarkovLauncherExecutableFilePath = "C:/Battlestate Games/BsgLauncher/BsgLauncher.exe",
                    TarkovResourcesFilePath = "C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets",
                    WebsiteBuildDirectory = "dist",
                    WebsiteCompilationCommand = "npm run build-test",
                    WebsiteDeploymentFileNotToDeletePattern = "data/.*",
                    WebsiteDirectoryPath = "D:/TotovBuilder/TotovBuilder.Website"
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            Deployer deployer = new Deployer(
                new Mock<IApplicationLogger<Deployer>>().Object,
                consoleWrapperMock.Object,
                promptWrapperMock.Object,
                configurationLoaderMock.Object,
                configuration,
                new Mock<IDeploymentAction<CompileWebsiteAction>>().Object,
                new Mock<IDeploymentAction<DeployRawDataAction>>().Object,
                new Mock<IDeploymentAction<DeployWebsiteAction>>().Object,
                new Mock<IDeploymentAction<ExtractTarkovDataAction>>().Object,
                new Mock<IDeploymentAction<UpdateTarkovAction>>().Object);

            // Act
            await deployer.Run();

            // Assert            
            configurationLoaderMock.Verify();
            consoleWrapperMock.Verify(m => m.WriteLine(DeploymentMode.Production.ToString()!.ToUpperInvariant()));
        }

        [Fact]
        public async Task Run_WithConfigurationLoadingException_ShouldLogErrorAndAskToChooseDeploymentModeAgain()
        {
            // Arrange
            bool hasThrown = false;
            IApplicationConfiguration configuration = new ApplicationConfiguration();

            Mock<IApplicationLogger<Deployer>> loggerMock = new Mock<IApplicationLogger<Deployer>>();
            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            Mock<IPromtWrapper> promptWrapperMock = new Mock<IPromtWrapper>();
            promptWrapperMock
                .Setup(m => m.Select(
                    "Deployment mode",
                    It.IsAny<DeploymentMode[]>()))
                .Returns(DeploymentMode.Test);
            promptWrapperMock
                .Setup(m => m.Select(
                    "Select an action",
                    It.IsAny<IEnumerable<string>>()))
                .Returns("     Exit");

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();
            configurationLoaderMock
                .Setup(m => m.Load(DeploymentMode.Test))
                .Callback(() =>
                {
                    if (!hasThrown)
                    {
                        hasThrown = true;

                        throw new Exception("Configuration error");
                    }

                    configuration.DeployerConfiguration = new DeployerConfiguration()
                    {
                        AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json",
                        ConfigurationsDirectory = $"../../../../../../TotovBuilder.Configuration\\{DeploymentMode.Test.ToString().ToUpperInvariant()}",
                        DeployerConfigurationFileName = "deployer-configuration.json",
                        DeployerDeploymentMode = DeploymentMode.Test,
                        ItemsExtractionEndSearchString = "LocalProfile",
                        ItemsExtractionStartSearchString = "TestItemTemplates",
                        PreviousExtractionsArchiveDirectory = "archive",
                        TarkovLauncherExecutableFilePath = "C:/Battlestate Games/BsgLauncher/BsgLauncher.exe",
                        TarkovResourcesFilePath = "C:/Battlestate Games/EFT (live)/EscapeFromTarkov_Data/resources.assets",
                        WebsiteBuildDirectory = "dist",
                        WebsiteCompilationCommand = "npm run build-test",
                        WebsiteDeploymentFileNotToDeletePattern = "data/.*",
                        WebsiteDirectoryPath = "D:/TotovBuilder/TotovBuilder.Website"
                    };
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            Deployer deployer = new Deployer(
                loggerMock.Object,
                consoleWrapperMock.Object,
                promptWrapperMock.Object,
                configurationLoaderMock.Object,
                configuration,
                new Mock<IDeploymentAction<CompileWebsiteAction>>().Object,
                new Mock<IDeploymentAction<DeployRawDataAction>>().Object,
                new Mock<IDeploymentAction<DeployWebsiteAction>>().Object,
                new Mock<IDeploymentAction<ExtractTarkovDataAction>>().Object,
                new Mock<IDeploymentAction<UpdateTarkovAction>>().Object);

            // Act
            await deployer.Run();

            // Assert
            configurationLoaderMock.Verify();
            consoleWrapperMock.Verify(m => m.WriteLine(DeploymentMode.Test.ToString()!.ToUpperInvariant()));
            loggerMock.Verify(m => m.LogError(It.IsAny<string>()));
        }

        [Fact]
        public async Task Run_ShouldExecuteAction()
        {
            // Arrange
            bool hasExecutedAction = false;
            IApplicationConfiguration configuration = new ApplicationConfiguration();

            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            Mock<IPromtWrapper> promptWrapperMock = new Mock<IPromtWrapper>();
            promptWrapperMock
                .Setup(m => m.Select(
                    "Deployment mode",
                    It.IsAny<DeploymentMode[]>()))
                .Returns(DeploymentMode.Test);
            promptWrapperMock
                .Setup(m => m.Select(
                    "Select an action",
                    It.IsAny<IEnumerable<string>>()))
                .Returns(() =>
                {
                    if (!hasExecutedAction)
                    {
                        hasExecutedAction = true;

                        return " 4 - Check configuration files";
                    }

                    return "     Exit";
                });

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();

            Deployer deployer = new Deployer(
                new Mock<IApplicationLogger<Deployer>>().Object,
                consoleWrapperMock.Object,
                promptWrapperMock.Object,
                configurationLoaderMock.Object,
                configuration,
                new Mock<IDeploymentAction<CompileWebsiteAction>>().Object,
                new Mock<IDeploymentAction<DeployRawDataAction>>().Object,
                new Mock<IDeploymentAction<DeployWebsiteAction>>().Object,
                new Mock<IDeploymentAction<ExtractTarkovDataAction>>().Object,
                new Mock<IDeploymentAction<UpdateTarkovAction>>().Object);

            // Act
            await deployer.Run();

            // Assert
            configurationLoaderMock.Verify();
            consoleWrapperMock.Verify(m => m.WriteLine(@"In the ""TotovBuilder.Configuration"" directory, check each configuration file to make sure everything looks fine.

When deploying in PRODUCTION, use a diff tool to compare the PRODUCTION files with the TEST files to check if properties are still the same."));
        }

        [Fact]
        public async Task Run_WithActionException_ShouldLogErrorAndDisplayMenuAgain()
        {
            // Arrange
            bool hasExecutedAction = false;
            IApplicationConfiguration configuration = new ApplicationConfiguration();

            Mock<IApplicationLogger<Deployer>> loggerMock = new Mock<IApplicationLogger<Deployer>>();
            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            Mock<IPromtWrapper> promptWrapperMock = new Mock<IPromtWrapper>();
            promptWrapperMock
                .Setup(m => m.Select(
                    "Deployment mode",
                    It.IsAny<DeploymentMode[]>()))
                .Returns(DeploymentMode.Test);
            promptWrapperMock
                .Setup(m => m.Select(
                    "Select an action",
                    It.IsAny<IEnumerable<string>>()))
                .Returns(() =>
                {
                    if (!hasExecutedAction)
                    {
                        hasExecutedAction = true;

                        return " 5 - Compile the website";
                    }

                    return "     Exit";
                });

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();

            Mock<IDeploymentAction<CompileWebsiteAction>> compileWebsiteActionMock = new Mock<IDeploymentAction<CompileWebsiteAction>>();
            compileWebsiteActionMock.SetupGet(m => m.Caption).Returns(" 5 - Compile the website");
            compileWebsiteActionMock.Setup(m => m.ExecuteAction()).Throws(new Exception("Compilation error"));

            Deployer deployer = new Deployer(
                loggerMock.Object,
                consoleWrapperMock.Object,
                promptWrapperMock.Object,
                configurationLoaderMock.Object,
                configuration,
                compileWebsiteActionMock.Object,
                new Mock<IDeploymentAction<DeployRawDataAction>>().Object,
                new Mock<IDeploymentAction<DeployWebsiteAction>>().Object,
                new Mock<IDeploymentAction<ExtractTarkovDataAction>>().Object,
                new Mock<IDeploymentAction<UpdateTarkovAction>>().Object);

            // Act
            await deployer.Run();

            // Assert
            configurationLoaderMock.Verify();
            loggerMock.Verify(m => m.LogError(It.IsAny<string>()));
        }
    }
}
