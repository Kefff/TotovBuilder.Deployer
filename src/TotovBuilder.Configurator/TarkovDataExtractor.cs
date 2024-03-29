﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TotovBuilder.Configurator.Abstractions;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Configurator
{
    /// <summary>
    /// Represents a Tarkov data extractor.
    /// </summary>
    public class TarkovDataExtractor : ITarkovDataExtractor
    {

        /// <summary>
        /// Configuration reader.
        /// </summary>
        private readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// Initilizes a new instances of the <see cref="TarkovDataExtractor"/> class.
        /// </summary>
        /// <param name="configurationReader"></param>
        public TarkovDataExtractor(IConfigurationReader configurationReader)
        {
            ConfigurationReader = configurationReader;
        }

        /// <inheritdoc/>
        public async Task Extract()
        {
            Logger.LogInformation(string.Format(Properties.Resources.ReadingTarkovResourcesFile, ConfigurationReader.ConfiguratorConfiguration.TarkovResourcesFilePath));

            StringBuilder tarkovResourcesFileContentStringBuilder = new StringBuilder();

            using (StreamReader sr = new StreamReader(ConfigurationReader.ConfiguratorConfiguration.TarkovResourcesFilePath))
            {
                bool takeLines = false;
                string? line;

                while ((line = sr.ReadLine()) != null)
                {
                    // Reading only lines in the section that interests us
                    if (line.Contains(ConfigurationReader.ConfiguratorConfiguration.ItemsExtractionStartSearchString))
                    {
                        takeLines = true;
                    }

                    if (takeLines)
                    {
                        tarkovResourcesFileContentStringBuilder.AppendLine(line);
                    }

                    if (line.Contains(ConfigurationReader.ConfiguratorConfiguration.ItemsExtractionEndSearchString))
                    {
                        break;
                    }
                }
            }

            string tarkovResourcesFileContent = tarkovResourcesFileContentStringBuilder.ToString();
            string tarkovItemsJson = IsolateItemsInTarkovResourcesFileContent(tarkovResourcesFileContent);

            if (string.IsNullOrWhiteSpace(tarkovItemsJson))
            {
                throw new Exception(string.Format(Properties.Resources.CannotReadTarkovResourcesFileContent));
            }

            await ExtractItemMissingProperties(tarkovItemsJson);
        }

        /// <summary>
        /// Moves a file to the configurations directory archive directory and adds a timestamp in the file name.
        /// </summary>
        /// <param name="fileToArchivePath">Path of the file to archive.</param>
        private void ArchiveConfigurationFile(string fileToArchivePath)
        {
            string archiveDirectory = Path.Combine(
                ConfigurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                ConfigurationReader.ConfiguratorConfiguration.PreviousExtractionsArchiveDirectory);
            string fileName = Path.GetFileName(fileToArchivePath);
            string archivedFileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + fileName;

            Directory.CreateDirectory(archiveDirectory);

            if (File.Exists(fileToArchivePath))
            {
                string destinationPath = Path.Combine(archiveDirectory, archivedFileName);

                Logger.LogInformation(string.Format(Properties.Resources.ArchivingFile, fileToArchivePath, destinationPath));

                File.Move(fileToArchivePath, destinationPath);
            }
        }

        /// <summary>
        /// Deserializes item missing properties.
        /// </summary>
        /// <param name="tarkovItemsJson">Json string representing the items.</param>
        /// <returns>Items.</returns>
        private static IEnumerable<ItemMissingProperties> DeserializeItemMissingProperties(string tarkovItemsJson)
        {
            List<ItemMissingProperties> extractedItems = new List<ItemMissingProperties>();
            JsonElement itemsJson = JsonDocument.Parse(tarkovItemsJson).RootElement;

            foreach (JsonProperty itemJson in itemsJson.EnumerateObject())
            {
                ItemMissingProperties? itemMissingProperties = DeserializeItemMissingProperties(itemJson);

                if (itemMissingProperties != null)
                {
                    extractedItems.Add(itemMissingProperties);
                }
            }

            return extractedItems;
        }

        /// <summary>
        /// Deserializes the missing properties of an item.
        /// </summary>
        /// <param name="itemJson">Json property representing the item.</param>
        /// <returns>Item.</returns>
        private static ItemMissingProperties? DeserializeItemMissingProperties(JsonProperty itemJson)
        {
            ItemMissingProperties itemMissingProperties = new ItemMissingProperties()
            {
                Id = itemJson.Value.GetProperty("_id").GetString()!
            };

            JsonElement propsJson = itemJson.Value.GetProperty("_props");

            // MaxStackableAmount
            if (propsJson.TryGetProperty("StackMaxSize", out JsonElement stackMaxSizeJson))
            {
                itemMissingProperties.MaxStackableAmount = stackMaxSizeJson.GetDouble();
            };

            if (itemMissingProperties.MaxStackableAmount > 1)
            {
                return itemMissingProperties;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts the item missing properties and saves them in a file in the configurations directory.
        /// </summary>
        /// <param name="tarkovItemsJson">Json representing the tarkov items.</param>
        private Task ExtractItemMissingProperties(string tarkovItemsJson)
        {
            return Task.Run(() =>
            {
                Logger.LogInformation(string.Format(Properties.Resources.ExtractingItems));

                IEnumerable<ItemMissingProperties> items = DeserializeItemMissingProperties(tarkovItemsJson);
                string itemsJson = JsonSerializer.Serialize(items, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                string missingItemPropertiesFilePath = Path.Combine(
                    ConfigurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                    ConfigurationReader.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName);
                ArchiveConfigurationFile(missingItemPropertiesFilePath);
                File.WriteAllText(missingItemPropertiesFilePath, itemsJson);

                Logger.LogSuccess(string.Format(Properties.Resources.ItemsExtracted, items.Count()));
            });
        }

        /// <summary>
        /// Finds and isolates items in the Tarkov resource file content.
        /// </summary>
        /// <param name="tarkovResourcesFileContent">Tarkov resource file content.</param>
        /// <returns>Isolated items.</returns>
        private string IsolateItemsInTarkovResourcesFileContent(string tarkovResourcesFileContent)
        {
            // Deleting the start of the content
            int startIndex = tarkovResourcesFileContent.IndexOf(ConfigurationReader.ConfiguratorConfiguration.ItemsExtractionStartSearchString);

            if (startIndex >= 0)
            {
                startIndex = tarkovResourcesFileContent.IndexOf("\"data\": {", startIndex);
                startIndex = tarkovResourcesFileContent.IndexOf('{', startIndex);
                tarkovResourcesFileContent = tarkovResourcesFileContent[startIndex..];
            }

            // Deleting the end of the content
            int endIndex = tarkovResourcesFileContent.IndexOf(ConfigurationReader.ConfiguratorConfiguration.ItemsExtractionEndSearchString);

            if (endIndex >= 0)
            {
                tarkovResourcesFileContent = tarkovResourcesFileContent[..endIndex];
                endIndex = tarkovResourcesFileContent.LastIndexOf('}');
                tarkovResourcesFileContent = tarkovResourcesFileContent[..endIndex];
            }

            return tarkovResourcesFileContent;
        }
    }
}
