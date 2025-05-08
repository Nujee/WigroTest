using UnityEngine;
using UnityEngine.UI;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class InventoryView : MonoBehaviour
    {
        [field: SerializeField] public Canvas Canvas { get; private set; }
        [field: SerializeField] public InfoView InfoView { get; private set; }
        [field: SerializeField] public RectTransform MainPanel { get; private set; }
        [field: SerializeField] public Button ToggleInventoryButton { get; private set; }
        [field: SerializeField] public RectTransform HiddenTransform { get; private set; }
        [field: SerializeField] public RectTransform ShownTransform { get; private set; }
        [field: SerializeField] public RectTransform SlotsParent { get; private set; }
        [field: SerializeField] public SlotView SlotPrefab { get; private set; }
        [field: SerializeField] public ItemView ItemPrefab { get; private set; }

        private bool _isOpen;
        private (bool onOpen, bool onClose) _doAnimate;

        public InventoryView Init((bool onOpen, bool onClose) doAnimate)
        {
            _doAnimate = doAnimate;

            MainPanel.transform.position = HiddenTransform.position;

            ToggleInventoryButton.onClick.AddListener(ToggleInventoryState);

            return this;
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

        private void OnDestroy()
        {
            ToggleInventoryButton.onClick.RemoveAllListeners();
        }
    }
}