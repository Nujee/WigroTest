using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private bool _isInTransition;
        private int? _lastSelectedSlotId = null;
        private PointerEventData _beginDragEventDataCache;
        private PointerEventData _dragEventDataCache;

        private InfoViewDecorator _infoViewDecorator;
        private Settings _settings; // 3) c)
        [field: SerializeField] public InventoryViewUILinks UI { get; private set; }

        public event Action<int> OnSlotSelect = delegate { };
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

            UI.MainPanel.transform.position = UI.CloseTransform.position;

            _infoViewDecorator = new InfoViewDecorator(UI.InfoView, settings);
            _infoViewDecorator.Close();

            UI.ToggleInventoryButton.onClick.AddListener(ToggleInventoryState);

            return this;
        }

        #region IInventoryView methods

        public void InitializeSlot(int slotId)
        {
            var slotView = _slotViewPool.Get();
            _slotViewsById[slotId] = slotView;

            slotView.PaintBackground(UI.EmptySlotColor);
            slotView.Deselect();
        }

        public void EnableSlotInteraction(int slotId)
        {
            var slotView = _slotViewsById[slotId];

            slotView.OnClickEvent += OnClickProxy;
            slotView.OnBeginDragEvent += OnBeginDragProxy;
            slotView.OnDragEvent += OnDragProxy;
            slotView.OnEndDragEvent += OnEndDragProxy;
        }

        public void InitializeItem(string itemId)
        {
            var randomSprite = UI.ItemSprites[UnityEngine.Random.Range(0, UI.ItemSprites.Count)];
            var itemView = _itemViewPool.Get().Init(randomSprite);

            _itemViewsById[itemId] = itemView;
        }

        public void AttachItemToSlot(int slotId, string itemId)
        {
            var slotView = _slotViewsById[slotId];
            var itemView = _itemViewsById[itemId];

            itemView.transform.SetParent(slotView.transform);
            itemView.transform.localPosition = Vector2.zero;
        }

        public void RemoveItem(string itemId)
        {
            var itemView = _itemViewsById[itemId];
            _itemViewsById.Remove(itemId);
            _itemViewPool.Release(itemView);
        }

        public void BeginDragItem(int slotId, string itemId)
        {
            var itemView = _itemViewsById[itemId];
            itemView.transform.SetParent(UI.Canvas.transform, worldPositionStays: true);
            itemView.transform.position = _beginDragEventDataCache.position;

            var slotView = _slotViewsById[slotId];
            slotView.PaintBackground(UI.EmptySlotColor);

            LoseFocus();
        }

        public void DragItem(string itemId)
        {
            var itemView = _itemViewsById[itemId];
            itemView.transform.position = _dragEventDataCache.position;
        }

        public void ShowItemInfo(string itemId, int rarity)
        {
            _infoViewDecorator.Open();
            _infoViewDecorator.UpdateInfo(itemId, rarity);
        }

        public void UpdateSelection(int selectedSlotId)
        {
            if (_lastSelectedSlotId.HasValue)
            {
                var lastSelectedSlotView = _slotViewsById[_lastSelectedSlotId.Value];
                lastSelectedSlotView.Deselect();
            }

            var selectedSlotView = _slotViewsById[selectedSlotId];
            selectedSlotView.Select();
            _lastSelectedSlotId = selectedSlotId;
        }

        public void ApplyRarityVisualToSlot(int slotId, int? rarity)
        {
            var slotView = _slotViewsById[slotId];

            var color = rarity.HasValue
                ? UI.RarityColors.Find(rc => rc.Key == (ItemRarity)rarity).Value
                : UI.EmptySlotColor;

            slotView.PaintBackground(color);
        }

        #endregion

        #region Event proxy handlers

        private void OnClickProxy(SlotView slotView)
        {
            var slotId = _slotViewsById.GetKeyByValue(slotView);
            OnSlotSelect(slotId);
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
            if (_isInTransition)
                return;

            _isInTransition = true;
            UI.DisableInput();

            if (_isOpen)
                await CloseInventory();
            else
                await OpenInventory();

            _isOpen = !_isOpen;

            _isInTransition = false;
            UI.EnableInput();
        }

        private async Task OpenInventory()
        {
            await MoveInventoryOnToggle();
        }

        private async Task CloseInventory()
        {
            LoseFocus();
            await MoveInventoryOnToggle();
        }

        private async Task MoveInventoryOnToggle()
        {
            var doAnimate = _isOpen
                ? _settings.CloseAnimated
                : _settings.OpenAnimated;

            var targetPosition = _isOpen
                ? UI.CloseTransform.position
                : UI.OpenTransform.position;

            if (doAnimate)
                await UI.MainPanel.LinearMoveTo(targetPosition, _settings.AnimDuration);
            else
                UI.MainPanel.transform.position = targetPosition;
        }

        private void LoseFocus()
        {
            _infoViewDecorator.Close();
            ClearSelection();
        }

        private void ClearSelection()
        {
            if (!_lastSelectedSlotId.HasValue)
                return;

            _slotViewsById[_lastSelectedSlotId.Value].Deselect();
            _lastSelectedSlotId = null;
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