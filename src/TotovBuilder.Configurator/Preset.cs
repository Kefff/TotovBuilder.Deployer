using System;
using System.Collections.Generic;
using System.Linq;
using TotovBuilder.Model.Builds;

namespace TotovBuilder.Configurator
{
    /// <summary>
    /// Represents a preset.
    /// </summary>
    public class Preset
    {
        /// <summary>
        /// Items contained in the preset.
        /// </summary>
        public PresetItem[] Items { get; set; } = Array.Empty<PresetItem>();

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Converts the preset to an inventory item.
        /// </summary>
        /// <returns>Inventory item.</returns>
        public InventoryItem ToInventoryItem()
        {
            PresetItem mainItem = Items.Single(i => i.ParentID == null && i.SlotName == null);
            List<InventoryItemModSlot> modSlots = new();

            InventoryItem inventoryItem = new()
            {
                ItemId = mainItem.ItemId
            };

            foreach (PresetItem childItem in Items.Where(i => i.ParentID == mainItem.Id))
            {
                InventoryItemModSlot modSlot = childItem.ToInventoryItemModSlot(Items);
                modSlots.Add(modSlot);
            }

            inventoryItem.ModSlots = modSlots.ToArray();

            return inventoryItem;
        }
    }
}
