using UnityEngine;
using UnityEngine.UI;
using Wigro.Runtime;

[DisallowMultipleComponent]
public sealed class InventoryView : MonoBehaviour
{
    private bool _isOpen;

    [field: SerializeField] public RectTransform MainPanel { get; private set; }
    [field: SerializeField] public Button ToggleInventoryButton { get; private set; }
    [field: SerializeField] public RectTransform HiddenTransform { get; private set; }
    [field: SerializeField] public RectTransform ShownTransform { get; private set; }
    [field: SerializeField] public RectTransform SlotsParent { get; private set; }
    [field: SerializeField] public Slot SlotPrefab { get; private set; }

    public InventoryView Init(Settings settings)
    {
        MainPanel.transform.position = HiddenTransform.position;

        ToggleInventoryButton.onClick.AddListener(ToggleInventoryState);
        return this;

        async void ToggleInventoryState()
        {
            var targetPosition = _isOpen 
                ? HiddenTransform.position 
                : ShownTransform.position;

            var shouldAnimate = _isOpen 
                ? settings.CloseAnimated 
                : settings.OpenAnimated;

            _isOpen = !_isOpen;

            if (shouldAnimate)
                await MainPanel.LinearMoveTo(targetPosition, 0.5f);
            else
                MainPanel.transform.position = targetPosition;
        }
    }

    private void OnDestroy()
    {
        ToggleInventoryButton.onClick.RemoveAllListeners();
    }
}