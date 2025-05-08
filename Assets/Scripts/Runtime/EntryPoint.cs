using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class EntryPoint : MonoBehaviour
    {
        [field: SerializeField] public Settings Settings { get; private set; }
        [field: SerializeField] public InventoryView InventoryView { get; private set; }

        private void Start()
        {
            new Inventory(Settings, InventoryView);
        }
    }
}
