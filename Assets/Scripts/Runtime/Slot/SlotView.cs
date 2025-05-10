using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class SlotView : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [field: SerializeField] public Image Background { get; private set; }
        [field: SerializeField] public Color NeutralColor { get; private set; }

        public event Action<SlotView, PointerEventData> OnClicked = delegate { };
        public event Action<SlotView, PointerEventData> OnDragBegun = delegate { };
        public event Action<SlotView, PointerEventData> OnDragEvent = delegate { };
        public event Action<SlotView, PointerEventData> OnDragEnded = delegate { };

        public void OnPointerClick(PointerEventData eventData) => OnClicked(this, eventData);

        public void OnBeginDrag(PointerEventData eventData) => OnDragBegun(this, eventData);

        public void OnDrag(PointerEventData eventData) => OnDragEvent(this, eventData);

        public void OnEndDrag(PointerEventData eventData) => OnDragEnded(this, eventData);
    }
}