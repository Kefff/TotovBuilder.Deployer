using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to extract missing item properties from Tarkov data.
    /// </summary>
    public class ExtractTarkovDataAction : IDeploymentAction
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return "Extract missing item properties from Tarkov";
            }
        }

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger Logger;

        /// <summary>
        /// Initilizes a new instances of the <see cref="TarkovDataExtractor"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Application configuration.</param>
        public ExtractTarkovDataAction(IApplicationLogger logger, IApplicationConfiguration configuration)
        {
            Configuration = configuration;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task ExecuteAction()
        {
            Logger.LogInformation(string.Format(Properties.Resources.ReadingTarkovResourcesFile, Configuration.ConfiguratorConfiguration.TarkovResourcesFilePath));

            StringBuilder tarkovResourcesFileContentStringBuilder = new StringBuilder();

            using (StreamReader sr = new StreamReader(Configuration.ConfiguratorConfiguration.TarkovResourcesFilePath))
            {
                bool takeLines = false;
                string? line;

                while ((line = sr.ReadLine()) != null)
                {
                    // Reading only lines in the section that interests us
                    if (line.Contains(Configuration.ConfiguratorConfiguration.ItemsExtractionStartSearchString))
                    {
                        takeLines = true;
                    }

                    if (takeLines)
                    {
                        tarkovResourcesFileContentStringBuilder.AppendLine(line);
                    }

                    if (line.Contains(Configuration.ConfiguratorConfiguration.ItemsExtractionEndSearchString))
                    {
                        break;
                    }
                }
            }

            string tarkovResourcesFileContent = tarkovResourcesFileContentStringBuilder.ToString();
            string tarkovItemsJson = IsolateItemsInTarkovResourcesFileContent(tarkovResourcesFileContent);

            if (string.IsNullOrWhiteSpace(tarkovItemsJson))
            {
                Logger.LogError(string.Format(Properties.Resources.CannotReadTarkovResourcesFileContent));

                return;
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
                Configuration.ConfiguratorConfiguration.ConfigurationsDirectory,
                Configuration.ConfiguratorConfiguration.PreviousExtractionsArchiveDirectory);
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
                    Configuration.ConfiguratorConfiguration.ConfigurationsDirectory,
                    Configuration.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName);
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
            int startIndex = tarkovResourcesFileContent.IndexOf(Configuration.ConfiguratorConfiguration.ItemsExtractionStartSearchString);

            if (startIndex >= 0)
            {
                startIndex = tarkovResourcesFileContent.IndexOf("\"data\": {", startIndex);
                startIndex = tarkovResourcesFileContent.IndexOf('{', startIndex);
                tarkovResourcesFileContent = tarkovResourcesFileContent[startIndex..];
            }

            // Deleting the end of the content
            int endIndex = tarkovResourcesFileContent.IndexOf(Configuration.ConfiguratorConfiguration.ItemsExtractionEndSearchString);

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
