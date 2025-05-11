using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class InventoryView : MonoBehaviour, IInventoryView
    {
        private readonly Dictionary<int, SlotView> _slotViewsById = new();
        private readonly Dictionary<string, ItemView> _itemViewsById = new();

        private GenericPool<SlotView> _slotViewPool;
        private GenericPool<ItemView> _itemViewPool;

        private bool _isOpen; 
        private bool _inTransition;

        private PointerEventData _clickEventDataCache;
        private PointerEventData _beginDragEventDataCache;
        private PointerEventData _dragEventDataCache;

        private int? _lastSelectedSlotId = null;

        private Settings _settings; // 3) c)
        [field: SerializeField] public InventoryViewUILinks UI { get; private set; }

        public event Action<int> OnClick = delegate { };
        public event Action<int> OnBeginDrag = delegate { };
        public event Action<int> OnDrag = delegate { };
        public event Action<int> OnEndDragOutsideInventory = delegate { };
        public event Action<int, int> OnEndDragInDifferentSlot = delegate { };
        public event Action<int> OnEndDragReset = delegate { };

        public InventoryView Init(Settings settings, int itemsCount)
        {
            _settings = settings;

            _slotViewPool = new GenericPool<SlotView>(UI.SlotPrefab, _settings.Amount, UI.SlotsParent);
            _itemViewPool = new GenericPool<ItemView>(UI.ItemPrefab, itemsCount);

            UI.MainPanel.transform.position = UI.HiddenTransform.position;

            UI.ToggleInventoryButton.onClick.AddListener(ToggleInventoryState);

            return this;
        }

        public void SetupSlotView(int slotId)
        {
            var slotView = _slotViewPool.Get();

            _slotViewsById[slotId] = slotView;

            slotView.OnClickEvent += OnClickProxy;
            slotView.OnBeginDragEvent += OnBeginDragProxy;
            slotView.OnDragEvent += OnDragProxy;
            slotView.OnEndDragEvent += OnEndDragProxy;
        }

        public void SetupItemView(string itemId)
        {
            var randomSprite = UI.ItemSprites[UnityEngine.Random.Range(0, UI.ItemSprites.Count)];
            var itemView = _itemViewPool.Get().Init(randomSprite);

            _itemViewsById[itemId] = itemView;
        }

        public void AttachItemViewToSlotView(int slotId, string itemId)
        {
            if (!_slotViewsById.TryGetValue(slotId, out var slotView) ||
                !_itemViewsById.TryGetValue(itemId, out var itemView))
                return;

            itemView.transform.SetParent(slotView.transform);
            itemView.transform.localPosition = Vector2.zero;
        }

        public void RemoveItemView(string itemId)
        {
            if (!_itemViewsById.TryGetValue(itemId, out var itemView))
                return;

            _itemViewsById.Remove(itemId);
            _itemViewPool.Release(itemView);
        }

        public void ClickSlot(int slotId)
        {

        }

        public void BeginDragItem(string itemId)
        {
            if (!_itemViewsById.TryGetValue(itemId, out var itemView))
                return;

            itemView.transform.SetParent(UI.Canvas.transform, worldPositionStays: true);
            itemView.transform.position = _beginDragEventDataCache.position;
        }

        public void DragItem(string itemId)
        {
            if (!_itemViewsById.TryGetValue(itemId, out var itemView))
                return;

            itemView.transform.position = _dragEventDataCache.position;
        }

        private void OnSlotClick(SlotView view, PointerEventData data)
        {
            // slot frame - on
            // info view - settings.showinfo ? {on and update} : {}
        }

        public void UpdateSelection(int selectedSlotId)
        {
            if (_lastSelectedSlotId != null &&
                _slotViewsById.TryGetValue(_lastSelectedSlotId.Value, out var lastSelectedSlotView))
            {
                lastSelectedSlotView.Deselect();
            }

            // ¬ыбираем новый слот (если существует)
            if (_slotViewsById.TryGetValue(selectedSlotId, out var selectedSlot))
            {
                selectedSlot.Select();
                _lastSelectedSlotId = selectedSlotId;
            }
        }

        private void ClearSelection()
        {
            if (_lastSelectedSlotId == null)
                return;

            _slotViewsById[_lastSelectedSlotId.Value].Deselect();
            _lastSelectedSlotId = null;
        }

        private void OnClickProxy(SlotView slotView, PointerEventData data)
        {
            if (!_slotViewsById.TryGetKeyByValue(slotView, out var slotId))
                return;

            OnClick(slotId);
        }

        private void OnBeginDragProxy(SlotView slotView, PointerEventData data)
        {
            if (!_slotViewsById.TryGetKeyByValue(slotView, out var slotId))
                return;

            _beginDragEventDataCache = data;
            OnBeginDrag(slotId);

            //TODO: centralize infoview logic
            //InfoView.gameObject.SetActive(false); 
        }

        private void OnDragProxy(SlotView slotView, PointerEventData data)
        {
            if (!_slotViewsById.TryGetKeyByValue(slotView, out var slotId))
                return;

            _dragEventDataCache = data;
            OnDrag(slotId);
        }

        private void OnEndDragProxy(SlotView sourceSlotView, PointerEventData data)
        {
            if (!_slotViewsById.TryGetKeyByValue(sourceSlotView, out var sourceSlotId))
                return;

            if (IsOutsideInventory())
            {
                OnEndDragOutsideInventory(sourceSlotId);
                return;
            }

            SlotView targetSlotView = null;
            bool didHitDifferentSlot = TryRaycastHit(out var firstHit) &&
                                       TryGetSlotView(firstHit, out targetSlotView) &&
                                       IsDifferentSlotView(sourceSlotView, targetSlotView);

            if (didHitDifferentSlot)
            {
                if (!_slotViewsById.TryGetKeyByValue(targetSlotView, out var targetSlotId))
                    return;

                OnEndDragInDifferentSlot(sourceSlotId, targetSlotId);
                return;
            }

            OnEndDragReset(sourceSlotId);

            bool IsOutsideInventory()
            {
                return !RectTransformUtility.RectangleContainsScreenPoint(UI.MainPanel, data.position, UI.Canvas.worldCamera);
            }

            bool TryRaycastHit(out GameObject firstHit)
            {
                var raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(data, raycastResults);

                if (raycastResults.Count > 0)
                {
                    firstHit = raycastResults[0].gameObject;
                    return true;
                }

                firstHit = null;
                return false;
            }

            bool TryGetSlotView(GameObject firstHit, out SlotView targetSlotView)
            {
                var hitSlotView = firstHit.GetComponentInParent<SlotView>();

                if (hitSlotView != null)
                {
                    targetSlotView = hitSlotView;
                    return true;
                }

                targetSlotView = null;
                return false;
            }

            bool IsDifferentSlotView(SlotView source, SlotView target)
            {
                return source != target;
            }
        }

        private async void ToggleInventoryState()
        {
            if (_inTransition)
                return;

            _inTransition = true;

            var targetPosition = _isOpen
                ? UI.HiddenTransform.position
                : UI.ShownTransform.position;

            // 7)
            var shouldAnimate = _isOpen
                ? _settings.CloseAnimated
                : _settings.OpenAnimated;

            if (shouldAnimate)
                await UI.MainPanel.LinearMoveTo(targetPosition, _settings.AnimDuration);
            else
                UI.MainPanel.transform.position = targetPosition;

            _isOpen = !_isOpen;
            _inTransition = false;
        }

        private void OnDestroy()
        {
            foreach (var slotView in _slotViewsById.Values)
            {
                slotView.OnClickEvent -= OnSlotClick;
                slotView.OnBeginDragEvent -= OnBeginDragProxy;
                slotView.OnDragEvent -= OnDragProxy;
                slotView.OnEndDragEvent -= OnEndDragProxy;
            }

            UI.ToggleInventoryButton.onClick.RemoveAllListeners();

            _slotViewPool.Clear();
            _itemViewPool.Clear();
        }
    }
}