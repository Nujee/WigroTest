using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;
using Wigro.Runtime;

[DisallowMultipleComponent]
public sealed class Inventory : MonoBehaviour
{
    [field: SerializeField] public InventorySlot SlotPrefab { get; private set; }
    [field: SerializeField] public RectTransform SlotsParent { get; private set; }

    public void Init(Settings settings)
    {
        var slotsAmount = settings.Amount;
        var slotPool = new GenericPool<InventorySlot>(SlotPrefab, slotsAmount, SlotsParent);

        for (int i = 0; i < slotsAmount; ++i)
        {
            var slot = slotPool.Get();
            slot.Init();
        }
    }
}