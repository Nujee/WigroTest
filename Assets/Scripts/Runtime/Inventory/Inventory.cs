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
                    var itemView = _itemViewPool.Get(); //.Init();
                    itemView.Icon.sprite = _view.IconSprites[UnityEngine.Random.Range(0, _view.IconSprites.Count)];
                    itemView.transform.SetParent(slotView.transform);
                    itemView.transform.localPosition = Vector2.zero;

                    var itemData = itemDatas[i];

                    item = new Item(itemView, itemData);
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

            var itemTransform = slot.Item.View.transform;
            itemTransform.SetParent(_view.Canvas.transform, worldPositionStays: true);
            itemTransform.position = data.position;

            _view.InfoView.gameObject.SetActive(false);
        }

        private void OnSlotDrag(Slot slot, PointerEventData data)
        {
            if (slot.IsEmpty)
                return;

            var itemTransform = slot.Item.View.transform;
            itemTransform.position = data.position;
        }

        private void OnSlotDragEnd(SlotView sourceView, PointerEventData data)
        {
            var fromSlot = sourceView.Slot;

            if (fromSlot.IsEmpty) 
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
                    MoveItem(sourceView, targetView);
                else
                    SwapItems(sourceView, targetView);

            else if (isClickOutsideInventoryPanel)
                RemoveItem(fromSlot);

            else
                ReturnItem(sourceView);
        }

        private void MoveItem(SlotView sourceView, SlotView targetView)
        {
            var sourceSlot = sourceView.Slot;
            var targetSlot = targetView.Slot;

            var sourceItemTransform = sourceSlot.Item.View.transform;
            sourceItemTransform.SetParent(targetView.transform);
            sourceItemTransform.localPosition = Vector2.zero;

            targetSlot.SetItem(sourceSlot.Item);
            sourceSlot.Clear();
        }

        private void SwapItems(SlotView sourceView, SlotView targetView)
        {
            var sourceSlot = sourceView.Slot;
            var targetSlot = targetView.Slot;

            var sourceItemTransform = sourceSlot.Item.View.transform;
            sourceItemTransform.SetParent(targetView.transform);
            sourceItemTransform.localPosition = Vector2.zero;

            var targetItemTransform = targetSlot.Item.View.transform;
            targetItemTransform.SetParent(sourceView.transform);
            targetItemTransform.localPosition = Vector2.zero;

            var temp = targetSlot.Item;
            targetSlot.SetItem(sourceSlot.Item);
            sourceSlot.SetItem(temp);   
        }

        private void RemoveItem(Slot slot)
        {
            _itemViewPool.Release(slot.Item.View);
            slot.Clear();
        }

        private void ReturnItem(SlotView sourceView)
        {
            var itemTransform = sourceView.Slot.Item.View.transform;
            itemTransform.SetParent(sourceView.transform, true);
            itemTransform.localPosition = Vector2.zero;
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