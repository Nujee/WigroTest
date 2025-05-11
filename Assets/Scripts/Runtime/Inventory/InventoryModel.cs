using System;
using System.Collections.Generic;

namespace Wigro.Runtime
{
    public sealed class InventoryModel
    {
        public List<SlotModel> Slots { get; } = new();
        public List<ItemModel> Items { get; } = new();

        public void CreateSlots(int size)
        {
            for (var i = 0; i < size; i++)
            {
                var slot = new SlotModel(i);
                Slots.Add(slot);
            }
        }

        public void CreateItems(List<ItemData> database)
        {
            for (var j = 0; j < database.Count; j++)
            {
                var item = new ItemModel(database[j]);
                Items.Add(item);
            }
        }

        public void SetItemsToSlots()
        {
            // 10)
            var assignmentLimit = Math.Min(Slots.Count, Items.Count);

            for (var i = 0; i < assignmentLimit; i++)
            {
                var slot = Slots[i];
                var item = Items[i];

                slot.SetItem(item);
            }
        }

        // 6) b) II)
        public void MoveItem(SlotModel sourceSlot, SlotModel targetSlot)
        {
            targetSlot.SetItem(sourceSlot.AttachedItem);
            sourceSlot.Clear();
        }

        // 6) b) II)
        public void SwapItems(SlotModel sourceSlot, SlotModel targetSlot)
        {
            var temp = targetSlot.AttachedItem;
            targetSlot.SetItem(sourceSlot.AttachedItem);
            sourceSlot.SetItem(temp);   
        }

        // 6) b) III)
        public void RemoveItem(SlotModel slot)
        {
            slot.Clear();
        }

        public void ResetItem(SlotModel slot)
        {
            slot.SetItem(slot.AttachedItem);
        }
    }
}