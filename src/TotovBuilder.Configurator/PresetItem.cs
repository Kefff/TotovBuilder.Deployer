using System.Collections.Generic;
using System.Linq;
using TotovBuilder.Model.Builds;

namespace TotovBuilder.Configurator
{
    /// <summary>
    /// Represents a preset item.
    /// </summary>
    public class PresetItem
    {
        /// <summary>
        /// ID of the preset item (not the ID of the item).
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// ID of the item.
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// ID of the parent preset item.
        /// </summary>
        public string? ParentID { get; set; }

        /// <summary>
        /// Name of the mod slot occupied in the parent item.
        /// </summary>
        public string? SlotName { get; set; }

        /// <summary>
        /// Converts the preset item to an inventory item mod slot.
        /// </summary>
        /// <param name="presetItems">List of preset items which can contain child items.</param>
        /// <returns>Inventory item mod slot</returns>
        public InventoryItemModSlot ToInventoryItemModSlot(IEnumerable<PresetItem> presetItems)
        {
            InventoryItem inventoryItem = new()
            {
                ItemId = ItemId
            };
            List<InventoryItemModSlot> childModSlots = new();

            foreach (PresetItem childItem in presetItems.Where(i => i.ParentID == Id))
            {
                InventoryItemModSlot childModSlot = childItem.ToInventoryItemModSlot(presetItems);
                childModSlots.Add(childModSlot);
            }

            inventoryItem.ModSlots = childModSlots.ToArray();
            InventoryItemModSlot modSlot = new()
            {
                Item = inventoryItem,
                ModSlotName = SlotName!
            };

            return modSlot;
        }
    }
}
