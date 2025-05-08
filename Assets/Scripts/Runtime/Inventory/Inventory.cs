using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wigro.Runtime
{
    public sealed class Inventory : IDisposable
    {
        private readonly List<Slot> _slots = new();
        private InventoryView _view; 

        public Inventory(Settings settings, InventoryView view)
        {
            var doAnimate = (settings.OpenAnimated, settings.CloseAnimated);
            _view = view.Init(doAnimate);

            int slotsAmount = settings.Amount;
            var slotViewPool = new GenericPool<SlotView>(_view.SlotPrefab, slotsAmount, _view.SlotsParent);

            var itemDatas = new InnerDatabase(settings).ItemDatas;
            var itemViewPool = new GenericPool<ItemView>(_view.ItemPrefab, itemDatas.Count);

            for (var i = 0; i < slotsAmount; i++)
            {
                // Setup slot and its view
                var slot = new Slot();
                _slots.Add(slot);
                var slotView = slotViewPool.Get();
                slotView.Init(slot);

                // Register slot event handlers
                slotView.OnClicked += OnSlotClick;
                slotView.OnDragBegun += OnSlotDragBegin;
                slotView.OnDragInProcess += OnSlotDrag;
                slotView.OnDragEnded += OnSlotDragEnd;

                // Setup item
                Item item = null;
                var hasItemsRemaining = (i < itemDatas.Count);
                if (hasItemsRemaining)
                {
                    var itemView = itemViewPool.Get();
                    itemView.transform.SetParent(slotView.transform);
                    itemView.transform.localPosition = Vector2.zero;

                    var itemData = itemDatas[i];

                    item = new Item(itemView, itemData);
                }

                // Append item to slot
                slot.SetItem(item);
            }
        }

        private void OnSlotClick(Slot sourceSlot, PointerEventData data)
        {
            // add frame to slot
            // update info values
            _view.InfoView.gameObject.SetActive(true);
        }

        private void OnSlotDragBegin(Slot sourceSlot, PointerEventData data)
        {
            if (sourceSlot.IsEmpty)
                return;

            var iconTransform = sourceSlot.Item.View.Icon.transform;
            iconTransform.SetParent(_view.Canvas.transform, worldPositionStays: true);
            iconTransform.position = data.position;

            _view.InfoView.gameObject.SetActive(false);
        }

        private void OnSlotDrag(Slot sourceSlot, PointerEventData data)
        {
            if (sourceSlot.IsEmpty)
                return;

            var iconTransform = sourceSlot.Item.View.Icon.transform;
            iconTransform.position = data.position;
        }

        private void OnSlotDragEnd(Slot slot, PointerEventData data)
        {
            if (slot.IsEmpty) 
                return;

            var temp = slot.Item;
            //otherSlot

            //if (slot.IsEmpty)
            //{
            //    slot.SetItem(_draggedItem);
            //}
            //else
            //{
            //    var temp = slot.Item;
            //    slot.SetItem(_draggedItem);
            //    _draggedSlot.SetItem(temp);
            //}

            //// Вернуть иконку на своё место
            //_draggedItem.View.transform.SetParent(_draggedSlot.transform);
            //_draggedItem.View.transform.localPosition = Vector3.zero;

            //// Очистить состояние
            //_draggedSlot = null;
            //_draggedItem = null;
        }

        public void Dispose()
        {
            foreach (var slot in _slots)
            {
                //slot.View.OnClicked -= DoOnSlotClick;
                //slot.View.OnDragBegun -= DoOnSlotDragBegun;
                //slot.View.OnDragInProcess -= DragItem;
                //slot.View.OnDragEnded -= DoOnSlotDragEnded;
            }
        }
    }
}