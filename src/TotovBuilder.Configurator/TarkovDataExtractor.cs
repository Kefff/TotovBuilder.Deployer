using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TotovBuilder.Configurator.Abstractions;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using static System.Text.Json.JsonElement;

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

            string tarkovResourcesFileContent = await File.ReadAllTextAsync(ConfigurationReader.ConfiguratorConfiguration.TarkovResourcesFilePath);

            if (string.IsNullOrWhiteSpace(tarkovResourcesFileContent))
            {
                throw new Exception(string.Format(Properties.Resources.CannotReadTarkovResourcesFileContent));
            }
            
            Task.WaitAll(ExtractItems(tarkovResourcesFileContent), ExtractPresets(tarkovResourcesFileContent));
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
        /// Deserializes items.
        /// </summary>
        /// <param name="tarkovItemsJson">Json string representing the items.</param>
        /// <returns>Items.</returns>
        private IEnumerable<ItemMissingProperties> DeserializeItems(string tarkovItemsJson)
        {
            List<ItemMissingProperties> extractedItems = new List<ItemMissingProperties>();
            JsonElement itemsJson = JsonDocument.Parse(tarkovItemsJson).RootElement;

            foreach (JsonProperty itemJson in itemsJson.EnumerateObject())
            {
                ItemMissingProperties? itemMissingProperties = DeserializeItem(itemJson);

                if (itemMissingProperties != null)
                {
                    extractedItems.Add(itemMissingProperties);
                }
            }

            return extractedItems;
        }

        /// <summary>
        /// Deserializes an item.
        /// </summary>
        /// <param name="itemJson">Json property representing the item.</param>
        /// <returns>Item.</returns>
        private ItemMissingProperties? DeserializeItem(JsonProperty itemJson)
        {
            ItemMissingProperties itemMissingProperties = new ItemMissingProperties()
            {
                Id = itemJson.Value.GetProperty("_id").GetString()
            };

            JsonElement propsJson = itemJson.Value.GetProperty("_props");

            // MaxStackableAmount
            if (propsJson.TryGetProperty("StackMaxSize", out JsonElement stackMaxSizeJson))
            {
                itemMissingProperties.MaxStackableAmount = stackMaxSizeJson.GetDouble();
            };

            if (propsJson.TryGetProperty("Cartridges", out JsonElement cartridgesJson))
            {
                ArrayEnumerator cartridgesJsonEnumerator = cartridgesJson.EnumerateArray();

                if (cartridgesJsonEnumerator.Count() > 0)
                {
                    itemMissingProperties.AcceptedAmmunitionIds = cartridgesJsonEnumerator.First()
                        .GetProperty("_props")
                        .GetProperty("filters").EnumerateArray().First()
                        .GetProperty("Filter").EnumerateArray().Select(f => f.GetString())
                        .ToArray();
                }
            }

            // ConflictingItemIds
            if (propsJson.TryGetProperty("ConflictingItems", out JsonElement conflictingItemsJson))
            {
                itemMissingProperties.ConflictingItemIds = conflictingItemsJson.EnumerateArray().Select(ci => ci.GetString()).ToArray();
            }

            // Chambers & ModSLots
            List<ModSlot> modSlots = new List<ModSlot>();

            if (propsJson.TryGetProperty("Chambers", out JsonElement chambersJson))
            {
                ModSlot[] chambers = DeserializeItemModSlots(chambersJson);

                for (int i = 0; i < chambers.Length; i++)
                {
                    chambers[i].Name = "chamber" + i;
                }

                modSlots.AddRange(chambers);
            }

            if (propsJson.TryGetProperty("Slots", out JsonElement slotsJson))
            {
                modSlots.AddRange(DeserializeItemModSlots(slotsJson));
            }

            itemMissingProperties.ModSlots = modSlots.ToArray();

            // RicochetChance
            if (propsJson.TryGetProperty("RicochetParams", out JsonElement ricochetParamsJson))
            {
                itemMissingProperties.RicochetXValue = ricochetParamsJson.GetProperty("x").GetDouble();
            }

            if (itemMissingProperties.AcceptedAmmunitionIds.Length > 0
                || itemMissingProperties.ConflictingItemIds.Length > 0
                || itemMissingProperties.MaxStackableAmount > 1
                || itemMissingProperties.ModSlots.Length > 0)
            {
                return itemMissingProperties;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Deserializes the mod slots of an item.
        /// </summary>
        /// <param name="slotsJson">Json element representing the mod slots.</param>
        /// <returns>Mods slots.</returns>
        private ModSlot[] DeserializeItemModSlots(JsonElement slotsJson)
        {
            List<ModSlot> modSlots = new List<ModSlot>();

            foreach (JsonElement slotJson in slotsJson.EnumerateArray())
            {
                ModSlot modSlot = new ModSlot()
                {
                    Id = slotJson.GetProperty("_id").GetString(),
                    Name = slotJson.GetProperty("_name").GetString(),
                    Required = slotJson.GetProperty("_required").GetBoolean(),
                    CompatibleItemIds = slotJson
                        .GetProperty("_props")
                        .GetProperty("filters").EnumerateArray().First()
                        .GetProperty("Filter").EnumerateArray().Select(f => f.GetString())
                        .ToArray()
                };

                modSlots.Add(modSlot);
            }

            return modSlots.ToArray();
        }

        /// <summary>
        /// Deserializes presets.
        /// </summary>
        /// <param name="tarkovPresetsJson">Json string representing the presets.</param>
        /// <returns>Presets.</returns>
        private IEnumerable<InventoryItem> DeserializePresets(string tarkovPresetsJson)
        {
            List<Preset> extractedPresets = new List<Preset>();
            JsonElement presetsJson = JsonDocument.Parse(tarkovPresetsJson).RootElement;

            foreach (JsonProperty presetJson in presetsJson.EnumerateObject())
            {
                Preset preset = new Preset()
                {
                    Name = presetJson.Value.GetProperty("_name").GetString(),
                    Items = presetJson.Value.GetProperty("_items").EnumerateArray().Select(pi => DeserializePresetItem(pi)).ToArray()
                };

                extractedPresets.Add(preset);
            }

            IEnumerable<InventoryItem> presets = extractedPresets
                .Where(p => p.Name.ToLowerInvariant().EndsWith("default") || ConfigurationReader.ConfiguratorConfiguration.NonStandardPresetNames.Contains(p.Name))
                .Select(p => p.ToInventoryItem());

            return presets;
        }

        /// <summary>
        /// Deserializes a preset item.
        /// </summary>
        /// <param name="presetItemJson">Json element representing the preset item.</param>
        /// <returns>Preset item.</returns>
        private PresetItem DeserializePresetItem(JsonElement presetItemJson)
        {
            PresetItem presetItem = new PresetItem()
            {
                Id = presetItemJson.GetProperty("_id").GetString(),
                ItemId = presetItemJson.GetProperty("_tpl").GetString()
            };

            if (presetItemJson.TryGetProperty("parentId", out JsonElement parentIdJson))
            {
                presetItem.ParentID = parentIdJson.GetString();
            }

            if (presetItemJson.TryGetProperty("slotId", out JsonElement slotIdJson))
            {
                presetItem.SlotName = slotIdJson.GetString();
            }

            return presetItem;
        }

        /// <summary>
        /// Extracts the items and saves them in a file in the configurations directory.
        /// </summary>
        /// <param name="tarkovResourcesFileContent">Tarkov resource file content.</param>
        private Task ExtractItems(string tarkovResourcesFileContent)
        {
            return Task.Run(() =>
            {
                Logger.LogInformation(string.Format(Properties.Resources.ExtractingItems));

                string tarkovItemsJson = IsolateItemsInTarkovResourcesFileContent(tarkovResourcesFileContent);
                IEnumerable<ItemMissingProperties> items = DeserializeItems(tarkovItemsJson);
                string itemsJson = JsonSerializer.Serialize(items, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                string presetsFilePath = Path.Combine(
                    ConfigurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                    ConfigurationReader.AzureFunctionsConfiguration.AzureItemMissingPropertiesBlobName);
                ArchiveConfigurationFile(presetsFilePath);
                File.WriteAllText(presetsFilePath, itemsJson);

                Logger.LogSuccess(string.Format(Properties.Resources.ItemsExtracted, items.Count()));
            });
        }

        /// <summary>
        /// Extracts the presets and saves them in a file in the configurations directory.
        /// </summary>
        /// <param name="tarkovResourcesFileContent">Tarkov resource file content.</param>
        private Task ExtractPresets(string tarkovResourcesFileContent)
        {
            return Task.Run(() =>
            {
                Logger.LogInformation(string.Format(Properties.Resources.ExtractingPresets));

                string tarkovPresetsJson = IsolatePresetsInTarkovResourcesFileContent(tarkovResourcesFileContent);
                IEnumerable<InventoryItem> presets = DeserializePresets(tarkovPresetsJson);
                string presetsJson = JsonSerializer.Serialize(presets, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                string presetsFilePath = Path.Combine(
                    ConfigurationReader.ConfiguratorConfiguration.ConfigurationsDirectory,
                    ConfigurationReader.AzureFunctionsConfiguration.AzurePresetsBlobName);
                ArchiveConfigurationFile(presetsFilePath);
                File.WriteAllText(presetsFilePath, presetsJson);

                Logger.LogSuccess(string.Format(Properties.Resources.PresetsExtracted, presets.Count()));
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

        /// <summary>
        /// Finds and isolates presets in the Tarkov resource file content.
        /// </summary>
        /// <param name="tarkovResourcesFileContent">Tarkov resource file content.</param>
        /// <returns>Isolated prests.</returns>
        private string IsolatePresetsInTarkovResourcesFileContent(string tarkovResourcesFileContent)
        {
            // Deleting the start of the content
            int startIndex = tarkovResourcesFileContent.IndexOf(ConfigurationReader.ConfiguratorConfiguration.PresetsExtractionStartSearchString);

            if (startIndex >= 0)
            {
                startIndex = tarkovResourcesFileContent.IndexOf('{', startIndex);
                tarkovResourcesFileContent = tarkovResourcesFileContent[startIndex..];
            }

            // Deleting the end of the content
            int endIndex = tarkovResourcesFileContent.IndexOf(ConfigurationReader.ConfiguratorConfiguration.PresetsExtractionEndSearchString);

            if (endIndex >= 0)
            {
                tarkovResourcesFileContent = tarkovResourcesFileContent[..endIndex];
                endIndex = tarkovResourcesFileContent.LastIndexOf(',');
                tarkovResourcesFileContent = tarkovResourcesFileContent[..endIndex];
            }

            return tarkovResourcesFileContent;
        }
    }
}
