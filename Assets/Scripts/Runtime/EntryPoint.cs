using UnityEngine;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class EntryPoint : MonoBehaviour
    {
        [field: SerializeField] public Settings Settings { get; private set; }
        [field: SerializeField] public Inventory Inventory {  get; private set; }

        private void Start()
        {
            Inventory.Init(Settings);
        }
    }
}
