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
            var inventory = new Inventory().Init(Settings, InventoryView.Init(Settings));
        }
    }
}
