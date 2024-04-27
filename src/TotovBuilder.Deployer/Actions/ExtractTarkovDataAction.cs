using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to extract missing item properties from Tarkov data.
    /// </summary>
    public class ExtractTarkovDataAction : IDeploymentAction<ExtractTarkovDataAction>
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return Properties.Resources.ExtractTarkovDataAction;
            }
        }

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// File wrapper.
        /// </summary>
        private readonly IFileWrapper FileWrapper;

        /// <summary>
        /// Directory wrapper.
        /// </summary>
        private readonly IDirectoryWrapper DirectoryWrapper;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<ExtractTarkovDataAction> Logger;

        /// <summary>
        /// Stream reader wrapper factory.
        /// </summary>
        private readonly IStreamReaderWrapperFactory StreamReaderWrapperFactory;

        /// <summary>
        /// Initilizes a new instances of the <see cref="TarkovDataExtractor"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="fileWrapper">File wrapper.</param>
        /// <param name="directoryWrapper">Directory wrapper.</param>
        /// <param name="streamReaderWrapperFactory">Stream reader wrapper factory.</param>
        public ExtractTarkovDataAction(
            IApplicationLogger<ExtractTarkovDataAction> logger,
            IApplicationConfiguration configuration,
            IFileWrapper fileWrapper,
            IDirectoryWrapper directoryWrapper,
            IStreamReaderWrapperFactory streamReaderWrapperFactory)
        {
            Configuration = configuration;
            DirectoryWrapper = directoryWrapper;
            FileWrapper = fileWrapper;
            Logger = logger;
            StreamReaderWrapperFactory = streamReaderWrapperFactory;
        }

        /// <inheritdoc/>
        public async Task ExecuteAction()
        {
            Logger.LogInformation(string.Format(Properties.Resources.ReadingTarkovResourcesFile, Configuration.DeployerConfiguration.TarkovResourcesFilePath));

            StringBuilder tarkovResourcesFileContentStringBuilder = new StringBuilder();

            using (IStreamReaderWrapper srw = StreamReaderWrapperFactory.Create(Configuration.DeployerConfiguration.TarkovResourcesFilePath))
            {
                bool takeLines = false;
                string? line;

                while ((line = srw.ReadLine()) != null)
                {
                    // Reading only lines in the section that interests us
                    if (line.Contains(Configuration.DeployerConfiguration.ItemsExtractionStartSearchString))
                    {
                        takeLines = true;
                    }

                    if (takeLines)
                    {
                        tarkovResourcesFileContentStringBuilder.AppendLine(line);
                    }

                    if (line.Contains(Configuration.DeployerConfiguration.ItemsExtractionEndSearchString))
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
                Configuration.DeployerConfiguration.ConfigurationsDirectory,
                Configuration.DeployerConfiguration.PreviousExtractionsArchiveDirectory);
            string fileName = Path.GetFileName(fileToArchivePath);
            string archivedFileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + fileName;

            DirectoryWrapper.CreateDirectory(archiveDirectory);

            if (FileWrapper.Exists(fileToArchivePath))
            {
                string destinationPath = Path.Combine(archiveDirectory, archivedFileName);

                Logger.LogInformation(string.Format(Properties.Resources.ArchivingFile, fileToArchivePath, destinationPath));

                FileWrapper.Move(fileToArchivePath, destinationPath);

                Logger.LogInformation(string.Format(Properties.Resources.FileArchived, fileToArchivePath));
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
                Logger.LogInformation(string.Format(Properties.Resources.ExtractingMissingItemProperties));

                IEnumerable<ItemMissingProperties> items = DeserializeItemMissingProperties(tarkovItemsJson);
                string itemsJson = JsonSerializer.Serialize(items, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                string missingItemPropertiesFilePath = Path.Combine(
                    Configuration.DeployerConfiguration.ConfigurationsDirectory,
                    Configuration.AzureFunctionsConfiguration.RawItemMissingPropertiesBlobName);
                ArchiveConfigurationFile(missingItemPropertiesFilePath);
                FileWrapper.WriteAllText(missingItemPropertiesFilePath, itemsJson);

                Logger.LogSuccess(string.Format(Properties.Resources.MissingItemPropertiesExtracted, items.Count()));
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
            int startIndex = tarkovResourcesFileContent.IndexOf(Configuration.DeployerConfiguration.ItemsExtractionStartSearchString);

            if (startIndex >= 0)
            {
                startIndex = tarkovResourcesFileContent.IndexOf("\"data\": {", startIndex);
                startIndex = tarkovResourcesFileContent.IndexOf('{', startIndex);
                tarkovResourcesFileContent = tarkovResourcesFileContent[startIndex..];
            }

            // Deleting the end of the content
            int endIndex = tarkovResourcesFileContent.IndexOf(Configuration.DeployerConfiguration.ItemsExtractionEndSearchString);

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
