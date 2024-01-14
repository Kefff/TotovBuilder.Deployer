using TotovBuilder.Deployer.Actions;

namespace TotovBuilder.Deployer.Test.Actions
{
    /// <summary>
    /// Represents tests on the <see cref="ExtractTarkovDataAction"/> class.
    /// </summary>
    public class ExtractTarkovDataActionTest
    {
        //[Fact]
        //public async Task ExecuteAction_ShouldExtractItemMissingPropertiesAndArchiveOlderFile()
        //{
        //    // Arrange
        //    Mock<IFileWrapper> fileWrapperMock = new Mock<IFileWrapper>();

        //    ApplicationConfiguration configuration = new ApplicationConfiguration();

        //    ExtractTarkovDataAction extractTarkovDataAction = new ExtractTarkovDataAction(
        //        new Mock<IApplicationLogger<ExtractTarkovDataAction>>().Object,
        //        configuration,
        //        fileWrapperMock.Object);

        //    // Act
        //    await extractTarkovDataAction.ExecuteAction();

        //    string itemsJson = await File.ReadAllTextAsync(
        //        Path.Combine(
        //            configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
        //            configurationReader.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName));
        //    ItemMissingProperties[] items = JsonSerializer.Deserialize<ItemMissingProperties[]>(itemsJson, new JsonSerializerOptions()
        //    {
        //        PropertyNameCaseInsensitive = true
        //    })!;

        //    // Assert
        //    items.Length.Should().BeGreaterThan(0);

        //    foreach (ItemMissingProperties expectedItemMissingProperties in TestData.ItemMissingProperties)
        //    {
        //        ItemMissingProperties? item = items.SingleOrDefault(p => p.Id == expectedItemMissingProperties.Id);

        //        if (item == null)
        //        {
        //            throw new Exception($"Cannot find missing properties for item \"{expectedItemMissingProperties.Id}\"");
        //        }

        //        item.Should().BeEquivalentTo(expectedItemMissingProperties);
        //    }

        //    string[] archivedFiles = Directory.GetFiles(
        //        Path.Combine(
        //            configurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
        //            configurationReader.ConfiguratorConfiguration.PreviousExtractionsArchiveDirectory));
        //    archivedFiles.Any(f => f.EndsWith(configurationReader.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName)).Should().BeTrue();
        //}

        //[Fact]
        //public void ExecuteAction_WithInvalidTarkovResourceFileContent_ShouldThrow()
        //{
        //        // Arrange
        //        DeployerConfiguration configuratorConfiguration = new DeployerConfiguration()
        //        {
        //            TarkovResourcesFilePath = tarkovResourcesFilePath
        //        };

        //        Mock<IConfigurationLoader> configurationReaderMock = new Mock<IConfigurationLoader>();
        //        configurationReaderMock.SetupGet(m => m.ConfiguratorConfiguration).Returns(configuratorConfiguration);

        //        TarkovDataExtractor tarkovDataExtractor = new TarkovDataExtractor(configurationReaderMock.Object);

        //        // Act
        //        Func<Task> act = () => tarkovDataExtractor.Extract();

        //        // Assert
        //        act.Should().ThrowAsync<Exception>("");
        //}
    }
}
