using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wigro.Runtime
{
    [System.Serializable]
    public sealed class InventoryViewUILinks
    {
        [field: SerializeField] public Canvas Canvas { get; private set; }
        [field: SerializeField] public GraphicRaycaster Raycaster { get; private set; }
        [field: SerializeField] public InfoView InfoView { get; private set; }
        [field: SerializeField] public RectTransform MainPanel { get; private set; }
        [field: SerializeField] public Button ToggleInventoryButton { get; private set; }
        [field: SerializeField] public RectTransform CloseTransform { get; private set; }
        [field: SerializeField] public RectTransform OpenTransform { get; private set; }
        [field: SerializeField] public RectTransform SlotsParent { get; private set; }
        [field: SerializeField] public SlotView SlotPrefab { get; private set; }
        [field: SerializeField] public ItemView ItemPrefab { get; private set; }
        [field: SerializeField] public Color EmptySlotColor { get; private set; }
        [field: SerializeField] public List<KeyValue<ItemRarity, Color>> RarityColors { get; private set; }
        [field: SerializeField] public List<Sprite> ItemSprites { get; private set; }

        public void EnableInput() => ToggleInput(true);

        public void DisableInput() => ToggleInput(false);

        private void ToggleInput(bool isActive) => Raycaster.enabled = isActive;

        [System.Serializable]
        public sealed class KeyValue<TKey, TValue>
        {
            public TKey Key;
            public TValue Value;
        }
    }
}