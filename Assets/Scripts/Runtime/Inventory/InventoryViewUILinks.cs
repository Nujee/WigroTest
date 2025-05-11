using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wigro.Runtime;

[System.Serializable]
public sealed class InventoryViewUILinks
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
    [field: SerializeField] public List<Sprite> ItemSprites { get; private set; }
}

