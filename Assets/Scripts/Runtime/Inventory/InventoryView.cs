using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class InventoryView : MonoBehaviour, IInventoryView
    {
        #region View lookups and pools

        private readonly Dictionary<int, SlotView> _slotViewsById = new();
        private readonly Dictionary<string, ItemView> _itemViewsById = new();

        private GenericPool<SlotView> _slotViewPool;
        private GenericPool<ItemView> _itemViewPool;

        #endregion

        #region Cache

        private bool _isOpen; 
        private bool _inTransition;
        private int? _lastSelectedSlotId = null;
        private PointerEventData _beginDragEventDataCache;
        private PointerEventData _dragEventDataCache;

        #endregion

        #region Dependencies

        private Settings _settings; // 3) c)
        [field: SerializeField] public InventoryViewUILinks UI { get; private set; }

        #endregion

        #region Events

        public event Action<int> OnSlotClick = delegate { };
        public event Action<int> OnBeginDrag = delegate { };
        public event Action<int> OnDrag = delegate { };
        public event Action<int> OnEndDragOutsideInventory = delegate { };
        public event Action<int, int> OnEndDragInDifferentSlot = delegate { };
        public event Action<int> OnEndDragReset = delegate { };

        #endregion

        public InventoryView Init(Settings settings, int itemsCount)
        {
            _settings = settings;

            _slotViewPool = new GenericPool<SlotView>(UI.SlotPrefab, _settings.Amount, UI.SlotsParent);
            _itemViewPool = new GenericPool<ItemView>(UI.ItemPrefab, itemsCount);

            UI.MainPanel.transform.position = UI.HiddenTransform.position;
            UI.ToggleInventoryButton.onClick.AddListener(ToggleInventoryState);

            return this;
        }

        #region IInventoryView methods

        public void SetupSlotView(int slotId)
        {
            var slotView = _slotViewPool.Get();
            _slotViewsById[slotId] = slotView;
        }

        public void SubscribeToSlotInput(int slotId)
        {
            var slotView = _slotViewsById[slotId];

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
            var slotView = _slotViewsById[slotId];
            var itemView = _itemViewsById[itemId];

            itemView.transform.SetParent(slotView.transform);
            itemView.transform.localPosition = Vector2.zero;
        }

        public void RemoveItemView(string itemId)
        {
            var itemView = _itemViewsById[itemId];

            _itemViewsById.Remove(itemId);
            _itemViewPool.Release(itemView);
        }

        public void BeginDragItem(string itemId)
        {
            var itemView = _itemViewsById[itemId];

            itemView.transform.SetParent(UI.Canvas.transform, worldPositionStays: true);
            itemView.transform.position = _beginDragEventDataCache.position;

            UI.InfoView.Close();
            ClearSelection();
        }

        public void DragItem(string itemId)
        {
            var itemView = _itemViewsById[itemId];
            itemView.transform.position = _dragEventDataCache.position;
        }

        public void UpdateInfoPanel(string itemId, int rarity)
        {
            UI.InfoView.Show();
            UI.InfoView.UpdateInfo(itemId, rarity);
        }

        public void UpdateSelection(int selectedSlotId)
        {
            if (_lastSelectedSlotId != null)
            {
                var lastSelectedSlotView = _slotViewsById[_lastSelectedSlotId.Value];
                lastSelectedSlotView.Deselect();
            }

            var selectedSlotView = _slotViewsById[selectedSlotId];
            selectedSlotView.Select();
            _lastSelectedSlotId = selectedSlotId;
        }

        private void ClearSelection()
        {
            if (_lastSelectedSlotId == null)
                return;

            _slotViewsById[_lastSelectedSlotId.Value].Deselect();
            _lastSelectedSlotId = null;
        }

        #endregion

        #region Event proxy handlers

        private void OnClickProxy(SlotView slotView, PointerEventData data)
        {
            var slotId = _slotViewsById.GetKeyByValue(slotView);
            OnSlotClick(slotId);
        }

        private void OnBeginDragProxy(SlotView slotView, PointerEventData data)
        {
            var slotId = _slotViewsById.GetKeyByValue(slotView);
            _beginDragEventDataCache = data;
            OnBeginDrag(slotId);
        }

        private void OnDragProxy(SlotView slotView, PointerEventData data)
        {
            var slotId = _slotViewsById.GetKeyByValue(slotView);
            _dragEventDataCache = data;
            OnDrag(slotId);
        }

        private void OnEndDragProxy(SlotView sourceSlotView, PointerEventData data)
        {
            var sourceSlotId = _slotViewsById.GetKeyByValue(sourceSlotView);

            if (IsOutsideInventoryPanel())
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
                var targetSlotId = _slotViewsById.GetKeyByValue(targetSlotView);
                OnEndDragInDifferentSlot(sourceSlotId, targetSlotId);
                return;
            }

            OnEndDragReset(sourceSlotId);

            bool IsOutsideInventoryPanel()
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

        #endregion

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
                slotView.OnClickEvent -= OnClickProxy;
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