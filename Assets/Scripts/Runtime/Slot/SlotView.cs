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
        [field: SerializeField] public Image SelectionFrame{ get; private set; }

        public event Action<SlotView> OnClickEvent = delegate { };
        public event Action<SlotView, PointerEventData> OnBeginDragEvent = delegate { };
        public event Action<SlotView, PointerEventData> OnDragEvent = delegate { };
        public event Action<SlotView, PointerEventData> OnEndDragEvent = delegate { };

        public void OnPointerClick(PointerEventData eventData) => OnClickEvent(this);

        public void OnBeginDrag(PointerEventData eventData) => OnBeginDragEvent(this, eventData);

        public void OnDrag(PointerEventData eventData) => OnDragEvent(this, eventData);

        public void OnEndDrag(PointerEventData eventData) => OnEndDragEvent(this, eventData);

        public void Select() => ToggleFrame(true);

        public void Deselect() => ToggleFrame(false);

        private void ToggleFrame(bool isActive) => SelectionFrame.gameObject.SetActive(isActive);

        public void PaintBackground(Color toColor) => Background.color = toColor;
    }
}