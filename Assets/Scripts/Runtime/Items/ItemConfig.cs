using UnityEngine;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "Create/Config/Item")]
public sealed class ItemConfig : ScriptableObject
{
    [field: SerializeField] public ItemRarity Rarity { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
}
