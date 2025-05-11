using System;
using System.Collections.Generic;

namespace Wigro.Runtime
{
    public sealed class InventoryPresenter : IDisposable
    {
        private InventoryModel _model;
        private IInventoryView _view;
        private Settings _settings;

        public InventoryPresenter(InventoryModel model, IInventoryView view)
        {
            _model = model;
            _view = view;
        }

        public void Init(int inventorySize, List<ItemData> database, Settings settings)
        {
            //  extract all in methods
            _settings = settings;

            _model.CreateSlots(inventorySize);

            foreach (var slot in _model.Slots)
            {
                _view.SetupSlotView(slot.Id);
                slot.OnItemSet += HandleItemSet;
            }

            _model.CreateItems(database);

            foreach (var item in database)
                _view.SetupItemView(item.ItemId);

            _model.SetItemsToSlots();

            _view.OnClick += HandleClick;
            _view.OnBeginDrag += HandleBeginDrag;
            _view.OnDrag += HandleDrag;
            _view.OnEndDragOutsideInventory += HandleEndDragOutsideInventory;
            _view.OnEndDragInDifferentSlot += HandleEndDragInDifferentSlot;
            _view.OnEndDragReset += HandleEndDragReset;
        }

        private void HandleClick(int clickedSlotId)
        {
            if (!TryGetSlotModelBySlotId(clickedSlotId, out var clickedSlot) ||
                clickedSlot.IsEmpty)
                return;

            _view.UpdateSelection(clickedSlotId);

            // в целом - просто _view.UpdateSelection() будет достаточно. реализация там такая:
            // clickedSlot.Select();
            // foreach(otherSlot) => otherSlot.Deselect() ИЛИ lastSelectedSlot.Deselect()

            //if (!clickedSlot.IsEmpty)
            //{
            //   _view.UpdateInfo(clickedSlot.AttachedItem.Data.ItemId);
            //}

        }

        private void HandleBeginDrag(int sourceSlotId)
        {
            if (!TryGetItemIdBySlotId(sourceSlotId, out var itemId))
                return;

            _view.BeginDragItem(itemId);
        }

        private void HandleDrag(int sourceSlotId)
        {
            if (!TryGetItemIdBySlotId(sourceSlotId, out var itemId))
                return;

            _view.DragItem(itemId);
        }

        private void HandleEndDragOutsideInventory(int sourceSlotId)
        {
            if (!TryGetSlotModelBySlotId(sourceSlotId, out var sourceSlot) ||
                !TryGetItemIdBySlotId(sourceSlotId, out var itemId))
                return;

            _model.RemoveItem(sourceSlot);
            _view.RemoveItemView(itemId);
        }

        private void HandleEndDragInDifferentSlot(int sourceSlotId, int targetSlotId)
        {
            if (!TryGetSlotModelBySlotId(sourceSlotId, out var sourceSlot) ||
                !TryGetSlotModelBySlotId(targetSlotId, out var targetSlot))
                return;

            if (sourceSlot.IsEmpty)
                return;

            if (targetSlot.IsEmpty)
                _model.MoveItem(sourceSlot, targetSlot);
            else
                _model.SwapItems(sourceSlot, targetSlot);
        }

        private void HandleEndDragReset(int sourceSlotId)
        {
            if (!TryGetSlotModelBySlotId(sourceSlotId, out var sourceSlot))
                return;

            _model.ResetItem(sourceSlot);
        }

        private void HandleItemSet(SlotModel slot)
        {
            if (slot.IsEmpty)
                return;

            var itemId = slot.AttachedItem.Data.ItemId;
            _view.AttachItemViewToSlotView(slot.Id, itemId);
        }

        private bool TryGetItemIdBySlotId(int slotId, out string itemId)
        {
            var doesSlotExistAndNonEmpty =
                TryGetSlotModelBySlotId(slotId, out var slot) && !slot.IsEmpty;

            if (doesSlotExistAndNonEmpty)
            {
                itemId = slot.AttachedItem.Data.ItemId;
                return true;
            }

            itemId = string.Empty;
            return false;
        }

        private bool TryGetSlotModelBySlotId(int slotId, out SlotModel slotModel)
        {
            foreach (var slot in _model.Slots)
            {
                if (slot.Id == slotId)
                {
                    slotModel = slot;
                    return true;
                }
            }

            slotModel = null;
            return false;
        }

        public void Dispose()
        {
            foreach (var slot in _model.Slots)
                slot.OnItemSet -= HandleItemSet;

            _view.OnClick -= HandleClick;
            _view.OnBeginDrag -= HandleBeginDrag;
            _view.OnDrag -= HandleDrag;
            _view.OnEndDragOutsideInventory -= HandleEndDragOutsideInventory;
            _view.OnEndDragInDifferentSlot -= HandleEndDragInDifferentSlot;
            _view.OnEndDragReset -= HandleEndDragReset;
        }
    }
}