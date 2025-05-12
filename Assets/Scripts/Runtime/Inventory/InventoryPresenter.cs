using System;
using System.Collections.Generic;

namespace Wigro.Runtime
{
    public sealed class InventoryPresenter : IDisposable
    {
        private readonly InventoryModel _model;
        private readonly IInventoryView _view;

        public InventoryPresenter(InventoryModel model, IInventoryView view)
        {
            _model = model;
            _view = view;
        }

        public void Init(int inventorySize, List<ItemData> database)
        {
            _model.CreateSlots(inventorySize);

            foreach (var slot in _model.Slots)
            {
                _view.InitializeSlot(slot.Id);
                _view.EnableSlotInteraction(slot.Id);

                slot.OnItemSet += HandleItemSet;
            }

            _model.CreateItems(database);

            foreach (var item in database)
                _view.InitializeItem(item.ItemId);

            _model.SetItemsToSlots();

            SubscribeToViewInputEvents();
        }

        private void HandleItemSet(int slotId)
        {
            var slot = _model.Slots[slotId];

            int? rarity = null;
            if (!slot.IsEmpty)
            {
                var item = slot.AttachedItem;
                _view.AttachItemToSlot(slotId, item.Data.ItemId);

                rarity = item.Data.Rarity;
            }

            _view.ApplyRarityVisualToSlot(slotId, rarity);
        }

        private void SubscribeToViewInputEvents()
        {
            _view.OnSlotSelect += HandleSelectSlot;
            _view.OnBeginDrag += HandleBeginDrag;
            _view.OnDrag += HandleDrag;
            _view.OnEndDragOutsideInventory += HandleEndDragOutsideInventory;
            _view.OnEndDragInDifferentSlot += HandleEndDragInDifferentSlot;
            _view.OnEndDragReset += HandleEndDragReset;
        }

        private void HandleSelectSlot(int selectedSlotId)
        {
            var clickedSlot = _model.Slots[selectedSlotId];
            if (clickedSlot.IsEmpty)
                return;

            var attachedItem = clickedSlot.AttachedItem;
            _view.ShowItemInfo(attachedItem.Data.ItemId, clickedSlot.AttachedItem.Data.Rarity);

            _view.UpdateSelection(selectedSlotId);
        }

        private void HandleBeginDrag(int sourceSlotId)
        {
            var sourceSlot = _model.Slots[sourceSlotId];
            if (sourceSlot.IsEmpty) 
                return;
            
            var itemId = sourceSlot.AttachedItem.Data.ItemId;
            _view.BeginDragItem(sourceSlotId, itemId);
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
            _view.RemoveItem(itemId);
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

        public void Dispose()
        {
            foreach (var slot in _model.Slots)
                slot.OnItemSet -= HandleItemSet;

            _view.OnSlotSelect -= HandleSelectSlot;
            _view.OnBeginDrag -= HandleBeginDrag;
            _view.OnDrag -= HandleDrag;
            _view.OnEndDragOutsideInventory -= HandleEndDragOutsideInventory;
            _view.OnEndDragInDifferentSlot -= HandleEndDragInDifferentSlot;
            _view.OnEndDragReset -= HandleEndDragReset;
        }
    }
}