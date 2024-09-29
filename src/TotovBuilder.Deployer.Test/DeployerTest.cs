using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
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

            Mock<IConsoleWrapper> consoleWrapperMock = new();

            Mock<IPromtWrapper> promptWrapperMock = new();
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

            Mock<IConfigurationLoader> configurationLoaderMock = new();
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

            Deployer deployer = new(
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

            Mock<IConsoleWrapper> consoleWrapperMock = new();

            Mock<IPromtWrapper> promptWrapperMock = new();
            promptWrapperMock
                .Setup(m => m.Select(
                    "Deployment mode",
                    It.IsAny<DeploymentMode[]>()))
                .Returns(DeploymentMode.Production);
            promptWrapperMock
                .Setup(m => m.Input<string>("Are you sure you want to use deployment mode \"PRODUCTION\"? Confirm by typing \"Yes\""))
                .Returns(() =>
                {
                    if (!hasFailedConfirmation)
                    {
                        hasFailedConfirmation = true;

                        return "No";
                    }

                    return confirmationText;
                })
                .Verifiable();
            promptWrapperMock
                .Setup(m => m.Select(
                    "Select an action",
                    It.IsAny<IEnumerable<string>>()))
                .Returns("     Exit");

            Mock<IConfigurationLoader> configurationLoaderMock = new();
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

            Deployer deployer = new(
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

            Mock<IApplicationLogger<Deployer>> loggerMock = new();
            Mock<IConsoleWrapper> consoleWrapperMock = new();

            Mock<IPromtWrapper> promptWrapperMock = new();
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

            Mock<IConfigurationLoader> configurationLoaderMock = new();
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

            Deployer deployer = new(
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

        [Theory]
        [InlineData(" 3 - Update the changelog", @"In the ""TotovBuilder.Configuration"" directory, update the ""changelog.json"" file with new functionalities.

Make sure to set the right version number and language for each entry.")]
        [InlineData(" 4 - Update website version and check configuration files", @"In the ""TotovBuilder.Configuration"" directory, open ""WebsiteConfiguration"" and update the version of the website.

Check each configuration file to make sure everything looks fine.

When deploying in PRODUCTION, use a diff tool to compare the PRODUCTION files with the TEST files to check if properties are still the same.")]
        [InlineData(" 7 - Deploy Azure Functions to Azure", @"Azure Functions must manually be published from Visual Studio :
- Open the ""TotovBuilder.AzureFunctions"" solution
- Right click on the ""TotovBuilder.AzureFunctions"" project and choose ""Publish"".

Make sure to CHOOSE THE RIGHT PROFILE at the top before publishing.

When deploying in TEST, the ""TotovBuilder.AzureFunctions"" project can then be locally launched to immediatly update the website data files in the ""data"" folder of the website on Azure.")]
        [InlineData(" 9 - Purge the Content Delivery Network on Azure", @"The content delivery network of the website needs to be purged to make the new version of the website accessible as soon as possible.

On Azure :
- Open the storage account
- Choose ""Front Door and CDN""
- Select the website endpoint
- Click on ""Purge"", check ""Purge all"" and click ""Purge""")]
        [InlineData("10 - Check the website", @"After the update, launch the website in a browser and check that new functionalities are present and that everything works.")]
        [InlineData("11 - Update Git", @"After the website is updated and tested, the develop branch can be merged on the main branch with a new version tag.

In Git, for each project :
- Merge ""develop"" on ""main""
- Add a tag with the new version number on the head of the  ""main"" breanch
- Checkout the ""develop"" branch
- Push the ""main"" and ""develop"" branches")]
        [InlineData("12 - Annonce the update on Discord", "")]
        public async Task Run_ShouldExecuteAction(string actionCaption, string expected)
        {
            // Arrange
            bool hasExecutedAction = false;
            IApplicationConfiguration configuration = new ApplicationConfiguration();

            Mock<IConsoleWrapper> consoleWrapperMock = new();

            Mock<IPromtWrapper> promptWrapperMock = new();
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

                        return actionCaption;
                    }

                    return "     Exit";
                })
                .Verifiable();

            Mock<IConfigurationLoader> configurationLoaderMock = new();

            Deployer deployer = new(
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
            consoleWrapperMock.Verify(m => m.WriteLine(expected));
        }

        [Fact]
        public async Task Run_WithActionException_ShouldLogErrorAndDisplayMenuAgain()
        {
            // Arrange
            bool hasExecutedAction = false;
            IApplicationConfiguration configuration = new ApplicationConfiguration();

            Mock<IApplicationLogger<Deployer>> loggerMock = new();
            Mock<IConsoleWrapper> consoleWrapperMock = new();

            Mock<IPromtWrapper> promptWrapperMock = new();
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
                })
                .Verifiable();

            Mock<IConfigurationLoader> configurationLoaderMock = new();

            Mock<IDeploymentAction<CompileWebsiteAction>> compileWebsiteActionMock = new();
            compileWebsiteActionMock.SetupGet(m => m.Caption).Returns(" 5 - Compile the website");
            compileWebsiteActionMock.Setup(m => m.ExecuteAction()).Throws(new Exception("Compilation error"));

            Deployer deployer = new(
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
