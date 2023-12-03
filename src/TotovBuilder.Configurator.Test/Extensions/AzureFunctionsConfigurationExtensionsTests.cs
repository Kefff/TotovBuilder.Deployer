using System.Collections.Generic;
using FluentAssertions;
using TotovBuilder.Configurator.Extensions;
using TotovBuilder.Model.Configuration;
using Xunit;

namespace TotovBuilder.Configurator.Test.Extensions
{
    /// <summary>
    /// Represents tests on the <see cref="AzureFunctionsConfigurationExtensions"/> class.
    /// </summary>
    public class AzureFunctionsConfigurationExtensionsTests
    {
        [Fact]
        public void GetBlobToUploadNames_ShouldGetBlobNames()
        {
            // Arrange
            AzureFunctionsConfiguration azureFunctionsConfiguration = new AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json",
                RawArmorPenetrationsBlobName = "raw-armor-penetrations.json",
                RawChangelogBlobName = "raw-changelog.json",
                RawItemCategoriesBlobName = "raw-item-categories.json",
                RawItemMissingPropertiesBlobName = "raw-item-missing-properties.json",
                RawTarkovValuesBlobName = "raw-tarkov-values.json",
                RawWebsiteConfigurationBlobName = "raw-website-configuration.json",
                WebsiteChangelogBlobName = "website-changelog.json",
                WebsiteItemCategoriesBlobName = "website-item-categories.json",
                WebsiteItemsBlobName = "website-items.json",
                WebsitePresetsBlobName = "website-presets.json",
                WebsitePricesBlobName = "website-prices.json",
                WebsiteTarkovValuesBlobName = "website-tarkov-values.json",
                WebsiteWebsiteConfigurationBlobName = "website-website-configuration.json"
            };

            // Act
            IEnumerable<string> blobNames = AzureFunctionsConfigurationExtensions.GetBlobToUploadNames(azureFunctionsConfiguration);

            // Assert
            blobNames.Should().BeEquivalentTo(new string[]
            {
                "raw-armor-penetrations.json",
                "raw-changelog.json",
                "raw-item-categories.json",
                "raw-item-missing-properties.json",
                "raw-tarkov-values.json",
                "raw-website-configuration.json"
            });
        }
    }
}
