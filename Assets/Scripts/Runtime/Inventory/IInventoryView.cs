using System;

namespace Wigro.Runtime
{
    public interface IInventoryView
    {
        event Action<int> OnSlotSelect;
        event Action<int> OnBeginDrag;
        event Action<int> OnDrag;
        event Action<int> OnEndDragOutsideInventory;
        event Action<int, int> OnEndDragInDifferentSlot;
        event Action<int> OnEndDragReset;

        void InitializeSlot(int slotId);
        void EnableSlotInteraction(int slotId);
        void InitializeItem(string itemId);
        void AttachItemToSlot(int slotId, string itemId);
        void RemoveItem(string itemId);
        void BeginDragItem(int slotId,string itemId);
        void DragItem(string itemId);
        void ShowItemInfo(string itemId, int rarity);
        void UpdateSelection(int slotId);
        void ApplyRarityVisualToSlot(int slotId, int? rarity);
    }
}
