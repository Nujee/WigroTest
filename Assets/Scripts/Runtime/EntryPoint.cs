using UnityEngine;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class EntryPoint : MonoBehaviour
    {
        [field: SerializeField] public Settings Settings { get; private set; }
        [field: SerializeField] public InventoryView View { get; private set; }

        private void Start()
        {
            // 9)
            var database = new DatabaseLoader().Database;
            var doAnimate = (Settings.OpenAnimated, Settings.CloseAnimated);
            var inventorySize = Settings.Amount;

            var model = new InventoryModel();
            var presenter = new InventoryPresenter(model, View);

            View.Init(doAnimate, inventorySize, database.Count);
            presenter.Init(inventorySize, database);
        }
    }
}
