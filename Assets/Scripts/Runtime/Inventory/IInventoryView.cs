using System;

namespace Wigro.Runtime
{
    public interface IInventoryView
    {
        event Action<int> OnClick;
        event Action<int> OnBeginDrag;
        event Action<int> OnDrag;
        event Action<int> OnEndDragOutsideInventory;
        event Action<int, int> OnEndDragInDifferentSlot;
        event Action<int> OnEndDragReset;

        void SetupSlotView(int slotId);
        void SetupItemView(string itemId);
        void AttachItemViewToSlotView(int slotId, string itemId);
        void RemoveItemView(string itemId);
        void ClickSlot(int slotId);
        void BeginDragItem(string itemId);
        void DragItem(string itemId);
    }
}
