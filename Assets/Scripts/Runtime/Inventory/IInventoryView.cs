using System;
using UnityEngine.EventSystems;

namespace Wigro.Runtime
{
    public interface IInventoryView
    {
        event Action<SlotView, PointerEventData> OnDrag;

        void AttachItemViewToSlotView(int slotId, string itemId);
        void ReleaseItemViewToPool(string itemId);
        void SetupSlotViewById(int slotId);
        void SetupItemViewById(string itemId);

    }
}
