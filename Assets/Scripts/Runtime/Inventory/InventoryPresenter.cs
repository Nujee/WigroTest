using System;
using System.Collections.Generic;

namespace Wigro.Runtime
{
    public sealed class InventoryPresenter : IDisposable
    {
        private InventoryModel _model;
        private IInventoryView _view;

        public InventoryPresenter(InventoryModel model, IInventoryView view)
        {
            _model = model;
            _view = view;
        }

        public void Do(int inventorySize, List<ItemData> database)
        {
            // create slot models
            _model.CreateSlots(inventorySize);

            // 1) setup slotId -> slotView dict in _view
            // 2) tell view to update visual on item set in model
            foreach (var slot in _model.Slots)
            {
                _view.SetupSlotViewById(slot.Id);
                slot.OnItemSet += OnItemSetToSlot;
            }

            // 3) create item models
            _model.CreateItems(database);

            // 4) setup itemId -> itemView dict in _view
            foreach (var item in database)
                _view.SetupItemViewById(item.ItemId);

            // 5) slot.setItem(item)
            _model.SetItemsToSlots();

            // 6) slot.attachedItem.Onremove += pool.release
            foreach (var slot in _model.Slots)
            {
                if (slot.IsEmpty)
                    continue;

                slot.AttachedItem.OnRemove += OnItemRemove;
            }

            _view.OnDrag += HandleOnDrag;
        }

        private void HandleOnDrag(slotvi)

        private void OnItemSetToSlot(SlotModel slot)
        {
            if (slot.IsEmpty)
                return;

            var itemId = slot.AttachedItem.Data.ItemId;
            _view.AttachItemViewToSlotView(slot.Id, itemId);
        }

        private void OnItemRemove(ItemModel item)
        {
            _view.ReleaseItemViewToPool(item.Data.ItemId);
        }

        public void ProcessDrop()
        {


            if (isItemActive)
            {
                if (didHitDifferentSlot)
                {
                    if (isTargetSlotEmpty)
                    {
                        Move();
                    }

                    else
                    {
                        Swap();
                    }
                }
                else
                {
                    Fallback();
                }
            }
            else
            {
                Remove();
            }




            if (doRemove)
            {
                Remove();
                return;
            }

            if (didHitDifferentSlot)
            {
                if (isTargetSlotEmpty) 
                    Move();
                else 
                    Swap();
            }
            else ;
        }

        public void RemoveOrReturn()
        {

        }

        public void DropItem()
        {

        }

        public void Dispose()
        {

        }
    }
}