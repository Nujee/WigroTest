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
        private GenericPool<ItemView> _itemViewPool;

        public Inventory(Settings settings, InventoryView view)
        {
            var doAnimate = (settings.OpenAnimated, settings.CloseAnimated);
            _view = view.Init(doAnimate);

            int slotsAmount = settings.Amount;
            var slotViewPool = new GenericPool<SlotView>(_view.SlotPrefab, slotsAmount, _view.SlotsParent);

            var itemDatas = new InnerDatabase(settings).ItemDatas;
            _itemViewPool = new GenericPool<ItemView>(_view.ItemPrefab, itemDatas.Count);

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
                    var itemView = _itemViewPool.Get();
                    var randomSprite = _view.IconSprites[UnityEngine.Random.Range(0, _view.IconSprites.Count)];
                    itemView.Init(randomSprite);

                    item = new Item(itemView, itemDatas[i]);
                    slot.SetItem(item);
                }
            }
        }

        private void OnSlotClick(Slot slot, PointerEventData data)
        {
            // add frame to slot
            // update info values
            _view.InfoView.gameObject.SetActive(true);
        }

        private void OnSlotDragBegin(Slot slot, PointerEventData data)
        {
            if (slot.IsEmpty)
                return;

            var itemTransform = slot.AttachedItem.View.transform;
            itemTransform.SetParent(_view.Canvas.transform, worldPositionStays: true);
            itemTransform.position = data.position;

            _view.InfoView.gameObject.SetActive(false);
        }

        private void OnSlotDrag(Slot slot, PointerEventData data)
        {
            if (slot.IsEmpty)
                return;

            var itemTransform = slot.AttachedItem.View.transform;
            itemTransform.position = data.position;
        }

        private void OnSlotDragEnd(SlotView sourceView, PointerEventData data)
        {
            var sourceSlot = sourceView.Slot;

            if (sourceSlot.IsEmpty) 
                return;

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, raycastResults);

            var uiUnderCursor = raycastResults.Count > 0 
                ? raycastResults[0].gameObject 
                : null;

            var targetView = uiUnderCursor?.GetComponentInParent<SlotView>();
            var isClickOutsideInventoryPanel =
                !RectTransformUtility.RectangleContainsScreenPoint(_view.MainPanel, data.position, _view.Canvas.worldCamera);

            if (targetView != null && targetView != sourceView)
                if (targetView.Slot.IsEmpty)
                    MoveItem(sourceSlot, targetView.Slot);
                else
                    SwapItems(sourceSlot, targetView.Slot);

            else if (isClickOutsideInventoryPanel)
                RemoveItem(sourceSlot);

            else
                ReturnItem(sourceSlot);
        }

        private void MoveItem(Slot sourceSlot, Slot targetSlot)
        {
            targetSlot.SetItem(sourceSlot.AttachedItem);
            sourceSlot.Clear();
        }

        private void SwapItems(Slot sourceSlot, Slot targetSlot)
        {
            var temp = targetSlot.AttachedItem;
            targetSlot.SetItem(sourceSlot.AttachedItem);
            sourceSlot.SetItem(temp);   
        }

        private void RemoveItem(Slot slot)
        {
            _itemViewPool.Release(slot.AttachedItem.View);
            slot.Clear();
        }

        private void ReturnItem(Slot slot)
        {
            slot.SetItem(slot.AttachedItem);
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