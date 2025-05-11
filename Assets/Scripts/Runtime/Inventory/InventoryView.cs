using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class InventoryView : MonoBehaviour, IInventoryView
    {
        private readonly Dictionary<int, SlotView> _slotViewsById = new();
        private readonly Dictionary<string, ItemView> _itemViewsById = new();

        private bool _isOpen;
        private (bool onOpen, bool onClose) _doAnimate;

        private GenericPool<SlotView> _slotViewPool;
        private GenericPool<ItemView> _itemViewPool;

        private PointerEventData _lastClickEventData;
        private PointerEventData _lastBeginDragEventData;
        private PointerEventData _lastDragEventData;

        [field: SerializeField] public Canvas Canvas { get; private set; }
        [field: SerializeField] public InfoView InfoView { get; private set; }
        [field: SerializeField] public RectTransform MainPanel { get; private set; }
        [field: SerializeField] public Button ToggleInventoryButton { get; private set; }
        [field: SerializeField] public RectTransform HiddenTransform { get; private set; }
        [field: SerializeField] public RectTransform ShownTransform { get; private set; }
        [field: SerializeField] public RectTransform SlotsParent { get; private set; }
        [field: SerializeField] public SlotView SlotPrefab { get; private set; }
        [field: SerializeField] public ItemView ItemPrefab { get; private set; }
        [field: SerializeField] public List<Sprite> ItemSprites { get; private set; }

        public event Action<int> OnClick = delegate { };
        public event Action<int> OnBeginDrag = delegate { };
        public event Action<int> OnDrag = delegate { };
        public event Action<int> OnEndDragOutsideInventory = delegate { };
        public event Action<int, int> OnEndDragInDifferentSlot = delegate { };
        public event Action<int> OnEndDragReset = delegate { };

        public InventoryView Init((bool onOpen, bool onClose) doAnimate, int inventorySize, int itemsCount)
        {
            _doAnimate = doAnimate;

            _slotViewPool = new GenericPool<SlotView>(SlotPrefab, inventorySize, SlotsParent);
            _itemViewPool = new GenericPool<ItemView>(ItemPrefab, itemsCount);

            MainPanel.transform.position = HiddenTransform.position;

            ToggleInventoryButton.onClick.AddListener(ToggleInventoryState);

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
            var randomSprite = ItemSprites[UnityEngine.Random.Range(0, ItemSprites.Count)];
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

        public void ClickSlot(int slotId)
        {

        }

        public void BeginDragItem(string itemId)
        {
            var itemTransform = _itemViewsById[itemId].transform;
            itemTransform.SetParent(Canvas.transform, worldPositionStays: true);
            itemTransform.position = _lastBeginDragEventData.position;
        }

        public void DragItem(string itemId)
        {
            var itemTransform = _itemViewsById[itemId].transform;
            itemTransform.position = _lastDragEventData.position;
        }

        private void OnSlotClick(SlotView view, PointerEventData data)
        {
            // slot frame - on
            // info view - settings.showinfo ? {on and update} : {}
        }

        private void OnClickProxy(SlotView slotView, PointerEventData data)
        {
            //if (!_slotViewsById.TryGetKeyByValue(slotView, out var slotId))
            //    return;

            //_lastClickEventData = data;
            //OnBeginDrag(slotId);
        }

        private void OnBeginDragProxy(SlotView slotView, PointerEventData data)
        {
            if (!_slotViewsById.TryGetKeyByValue(slotView, out var slotId))
                return;

            _lastBeginDragEventData = data;
            OnBeginDrag(slotId);

            //TODO: centralize infoview logic
            //InfoView.gameObject.SetActive(false); 
        }

        private void OnDragProxy(SlotView slotView, PointerEventData data)
        {
            if (!_slotViewsById.TryGetKeyByValue(slotView, out var slotId))
                return;

            _lastDragEventData = data;
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
                return !RectTransformUtility.RectangleContainsScreenPoint(MainPanel, data.position, Canvas.worldCamera);
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
            var targetPosition = _isOpen
                ? HiddenTransform.position
                : ShownTransform.position;

            // 7)
            var shouldAnimate = _isOpen
                ? _doAnimate.onClose
                : _doAnimate.onOpen;

            _isOpen = !_isOpen;

            if (shouldAnimate)
                await MainPanel.LinearMoveTo(targetPosition, 0.5f);
            else
                MainPanel.transform.position = targetPosition;
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

            ToggleInventoryButton.onClick.RemoveAllListeners();

            _slotViewPool.Clear();
            _itemViewPool.Clear();
        }
    }
}