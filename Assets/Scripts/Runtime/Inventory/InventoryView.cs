using System;
using System.Collections.Generic;
using UnityEditor.Graphs;
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

        private InventoryPresenter _presenter;
        private bool _isOpen;
        private (bool onOpen, bool onClose) _doAnimate;

        private GenericPool<SlotView> _slotViewPool;
        private GenericPool<ItemView> _itemViewPool;

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

        public event Action<SlotView, PointerEventData> OnDrag = delegate { };

        public InventoryView Init(InventoryPresenter presenter, (bool onOpen, bool onClose) doAnimate, int inventorySize, int itemsCount)
        {
            _presenter = presenter;
            _doAnimate = doAnimate;

            _slotViewPool = new GenericPool<SlotView>(SlotPrefab, inventorySize, SlotsParent);
            _itemViewPool = new GenericPool<ItemView>(ItemPrefab, itemsCount);

            MainPanel.transform.position = HiddenTransform.position;

            ToggleInventoryButton.onClick.AddListener(ToggleInventoryState);

            return this;
        }

        public void SetupSlotViewById(int slotId)
        {
            var slotView = _slotViewPool.Get();

            _slotViewsById[slotId] = slotView;

            slotView.OnClicked += OnSlotClick;
            slotView.OnDragBegun += OnSlotDragBegin;
            slotView.OnDragEvent += OnDragProxy;
            slotView.OnDragEnded += OnSlotDragEnd;
        }

        public void SetupItemViewById(string itemId)
        {
            var randomSprite = ItemSprites[Random.Range(0, ItemSprites.Count)];
            var itemView = _itemViewPool.Get().Init(randomSprite);

            _itemViewsById[itemId] = itemView;
        }

        public void SetupItemViewsById(List<ItemData> database)
        {
            var itemsCount = database.Count;
            _itemViewPool = new GenericPool<ItemView>(ItemPrefab, itemsCount);

            for (var i = 0; i < itemsCount; i++)
            {
                var randomSprite = ItemSprites[Random.Range(0, ItemSprites.Count)];
                var itemView = _itemViewPool.Get().Init(randomSprite);

                var itemData = database[i];
                _itemViewsById[itemData.ItemId] = itemView;
            }
        }

        public void AttachItemViewToSlotView(int slotId, string itemId)
        {
            var slotView = _slotViewsById[slotId];
            var itemView = _itemViewsById[itemId];

            itemView.transform.SetParent(slotView.transform);
            itemView.transform.localPosition = Vector2.zero;
        }

        public void ReleaseItemViewToPool(string itemId)
        {
            var itemView = _itemViewsById[itemId];

            _itemViewsById.Remove(itemId);
            _itemViewPool.Release(itemView);
        }

        private void OnSlotClick(SlotView view, PointerEventData data)
        {
            // slot frame - on
            // info view - on
        }

        private void OnSlotDragBegin(SlotView slotView, PointerEventData data)
        {
            var itemTransform = GetItemTransform(slotView.Slot);
            itemTransform.SetParent(Canvas.transform, worldPositionStays: true);
            itemTransform.position = data.position;

            //TODO: centralize infoview logic
            //InfoView.gameObject.SetActive(false); 
        }

        private void OnDragProxy(SlotView slotView, PointerEventData data)// => OnDrag(slotView, data);
        {
            if (!_slotViewsById.TryGetKeyByValue(slotView, out var slotId))
                return;

            OnDrag(slotId);

            var itemId = slotView.Slot.AttachedItem.Data.ItemId;
            var itemTransform = _itemViewsById[itemId].transform;

            itemTransform.position = data.position;
        }

        private void OnSlotDragEnd(SlotView sourceSlotView, PointerEventData data)
        {
            if (IsInsideInventoryPanel())
            {
                if (TryGetFirstHit(out var firstHit))
                {
                    if (TryGetSlotView(out var targetSlotView))
                    {
                        if (targetSlotView.Slot.IsEmpty)
                        {
                            Move();
                        }
                        else if (targetSlotView != sourceSlotView)
                        {
                            Swap();
                        }
                        else
                        {
                            Return();
                        }
                    }
                    else
                    {
                        Return ();
                    }
                }
                else
                {
                    Return();
                }
            }
            else
            {
                Remove();
            }

            var a = IsInsideInventoryPanel();
            var b = GetFirstHit();
            var c = GetSlotView(b);

            _presenter.ProcessDrop(a, b, c);

            bool IsInsideInventoryPanel()
            {
                return RectTransformUtility.RectangleContainsScreenPoint(MainPanel, data.position, Canvas.worldCamera);
            }

            GameObject GetFirstHit()
            {
                var raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(data, raycastResults);
                return raycastResults[0].gameObject;
            }

            SlotView GetSlotView(GameObject firstHit)
            {
                var result = firstHit 
                    ? firstHit.GetComponent<SlotView>() 
                    : null;

                return result;
            }
        }

        private async void ToggleInventoryState()
        {
            var targetPosition = _isOpen
                ? HiddenTransform.position
                : ShownTransform.position;

            var shouldAnimate = _isOpen
                ? _doAnimate.onClose
                : _doAnimate.onOpen;

            _isOpen = !_isOpen;

            if (shouldAnimate)
                await MainPanel.LinearMoveTo(targetPosition, 0.5f);
            else
                MainPanel.transform.position = targetPosition;
        }

        private Transform GetItemTransform(SlotModel slot)
        {
            var itemId = slot.AttachedItem.Data.ItemId;
            return _itemViewsById[itemId].transform;
        }

        private void OnDestroy()
        {
            foreach (var slotView in _slotViewsById.Values)
            {
                slotView.OnClicked -= OnSlotClick;
                slotView.OnDragBegun -= OnSlotDragBegin;
                slotView.OnDragEvent -= OnSlotDrag;
                slotView.OnDragEnded -= OnSlotDragEnd;
            }

            ToggleInventoryButton.onClick.RemoveAllListeners();
        }
    }
}