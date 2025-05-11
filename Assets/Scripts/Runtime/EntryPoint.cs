using UnityEngine;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class EntryPoint : MonoBehaviour
    {
        [field: SerializeField] public Settings Settings { get; private set; }
        [field: SerializeField] public InventoryView InventoryView { get; private set; }

        // 11) c)
        private void Start()
        {
            // 9)
            var database = new DatabaseLoader().Database;

            var model = new InventoryModel();
            var presenter = new InventoryPresenter(model, InventoryView);

            InventoryView.Init(Settings, database.Count);
            presenter.Init(Settings.Amount, database, Settings);
        }
    }
}
