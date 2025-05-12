using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class SlotView : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float _clickDistanceThreshold = 1f;
        private const float _clickTimeThreshold = 0.3f;

        private Vector2 _lastDownPosition;
        private float _lastDownTime;

        [field: SerializeField] public Image Background { get; private set; }
        [field: SerializeField] public Image SelectionFrame{ get; private set; }

        public event Action<SlotView> OnClickEvent = delegate { };
        public event Action<SlotView, PointerEventData> OnBeginDragEvent = delegate { };
        public event Action<SlotView, PointerEventData> OnDragEvent = delegate { };
        public event Action<SlotView, PointerEventData> OnEndDragEvent = delegate { };

        // Unity seems to consider any Up after Down on the same object as a click,
        // no matter how far in time and space were Down and Up, hence is this Down+Up separate processing
        public void OnPointerDown(PointerEventData eventData)
        {
            _lastDownPosition = eventData.position;
            _lastDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var isWithinClickDistance = (Vector2.Distance(eventData.position, _lastDownPosition) < _clickDistanceThreshold);
            var isWithinClickTime = (Time.time - _lastDownTime < _clickTimeThreshold);

            if ( isWithinClickDistance && isWithinClickTime)
                OnClickEvent(this);
        }

        public void OnBeginDrag(PointerEventData eventData) => OnBeginDragEvent(this, eventData);

        public void OnDrag(PointerEventData eventData) => OnDragEvent(this, eventData);

        public void OnEndDrag(PointerEventData eventData) => OnEndDragEvent(this, eventData);

        public void Select() => ToggleFrame(true);

        public void Deselect() => ToggleFrame(false);

        private void ToggleFrame(bool isActive) => SelectionFrame.gameObject.SetActive(isActive);

        public void PaintBackground(Color toColor) => Background.color = toColor;

    }
}