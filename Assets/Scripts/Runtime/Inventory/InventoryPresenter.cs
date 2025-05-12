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
                _view.SubscribeToSlotInput(slot.Id);
                slot.OnItemSet += HandleItemSet;
            }

            _model.CreateItems(database);

            foreach (var item in database)
                _view.SetupItemView(item.ItemId);

            _model.SetItemsToSlots();

            _view.OnSlotClick += HandleSlotClick;
            _view.OnBeginDrag += HandleBeginDrag;
            _view.OnDrag += HandleDrag;
            _view.OnEndDragOutsideInventory += HandleEndDragOutsideInventory;
            _view.OnEndDragInDifferentSlot += HandleEndDragInDifferentSlot;
            _view.OnEndDragReset += HandleEndDragReset;
        }

        private void HandleSlotClick(int clickedSlotId)
        {
            var clickedSlot = _model.Slots[clickedSlotId];
            if (clickedSlot.IsEmpty)
                return;

            var attachedItem = clickedSlot.AttachedItem;
            _view.UpdateInfoPanel(attachedItem.Data.ItemId, attachedItem.Data.Rarity);

            _view.UpdateSelection(clickedSlotId);
        }

        private void HandleBeginDrag(int sourceSlotId)
        {
            var sourceSlot = _model.Slots[sourceSlotId];
            if (sourceSlot.IsEmpty) 
                return;

            var itemId = sourceSlot.AttachedItem.Data.ItemId;
            _view.BeginDragItem(itemId);
        }

        private void HandleDrag(int sourceSlotId)
        {
            var sourceSlot = _model.Slots[sourceSlotId];
            if (sourceSlot.IsEmpty)
                return;

            var itemId = sourceSlot.AttachedItem.Data.ItemId;
            _view.DragItem(itemId);
        }

        private void HandleEndDragOutsideInventory(int sourceSlotId)
        {
            var sourceSlot = _model.Slots[sourceSlotId];
            if (sourceSlot.IsEmpty)
                return;

            var itemId = sourceSlot.AttachedItem.Data.ItemId;

            _model.RemoveItem(sourceSlot);
            _view.RemoveItemView(itemId);
        }

        private void HandleEndDragInDifferentSlot(int sourceSlotId, int targetSlotId)
        {
            var sourceSlot = _model.Slots[sourceSlotId];
            var targetSlot = _model.Slots[targetSlotId];

            if (sourceSlot.IsEmpty)
                return;

            if (targetSlot.IsEmpty)
                _model.MoveItem(sourceSlot, targetSlot);
            else
                _model.SwapItems(sourceSlot, targetSlot);
        }

        private void HandleEndDragReset(int sourceSlotId)
        {
            var sourceSlot = _model.Slots[sourceSlotId];
            if (sourceSlot.IsEmpty)
                return;

            _model.ResetItem(sourceSlot);
        }

        private void HandleItemSet(int slotId)
        {
            var slot = _model.Slots[slotId];

            if (slot.IsEmpty)
                return;

            var itemId = slot.AttachedItem.Data.ItemId;
            _view.AttachItemViewToSlotView(slot.Id, itemId);
        }

        public void Dispose()
        {
            foreach (var slot in _model.Slots)
                slot.OnItemSet -= HandleItemSet;

            _view.OnSlotClick -= HandleSlotClick;
            _view.OnBeginDrag -= HandleBeginDrag;
            _view.OnDrag -= HandleDrag;
            _view.OnEndDragOutsideInventory -= HandleEndDragOutsideInventory;
            _view.OnEndDragInDifferentSlot -= HandleEndDragInDifferentSlot;
            _view.OnEndDragReset -= HandleEndDragReset;
        }
    }
}