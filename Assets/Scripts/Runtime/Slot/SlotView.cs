using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class SlotView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [field: SerializeField] public Image Background { get; private set; }
        [field: SerializeField] public Color NeutralColor { get; private set; }

        public Slot Slot { get; private set; }

        public Action<Slot, PointerEventData> OnClicked = delegate { };
        public Action<Slot, PointerEventData> OnDragBegun = delegate { };
        public Action<Slot, PointerEventData> OnDragInProcess = delegate { };
        public Action<SlotView, PointerEventData> OnDragEnded = delegate { };

        public void Init(Slot slot)
        {
            Slot = slot;
            Slot.OnItemSet += PlaceItem;
        }

        private void PlaceItem(Item item)
        {
            item.View.transform.SetParent(transform);
            item.View.transform.localPosition = Vector2.zero;
        }

        public void OnPointerClick(PointerEventData eventData) => OnClicked(Slot, eventData);
                                                                            
        public void OnBeginDrag(PointerEventData eventData) => OnDragBegun(Slot, eventData);
                                                                            
        public void OnDrag(PointerEventData eventData) => OnDragInProcess(Slot, eventData);
                                                                          
        public void OnEndDrag(PointerEventData eventData) => OnDragEnded(this, eventData);

        private void OnDestroy() => Slot.OnItemSet -= PlaceItem;
    }
}