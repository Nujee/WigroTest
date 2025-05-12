using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class EntryPoint : MonoBehaviour
    {
        [field: SerializeField] public Settings Settings { get; private set; }
        [field: SerializeField] public AssetReference SceneReference { get; private set; }

        private async void Start()
        {
            await LoadInventorySceneAsync();

            var inventoryView = FindObjectOfType<InventoryView>();

            if (inventoryView != null)
            {
                var database = new DatabaseLoader().Database;
                var model = new InventoryModel();
                var presenter = new InventoryPresenter(model, inventoryView);

                inventoryView.Init(Settings, database.Count);
                presenter.Init(Settings.Amount, database);
            }
            else Debug.LogError("InventoryView not found in loaded scene.");
        }

        private async Task LoadInventorySceneAsync()
        {
            var asyncOperation = SceneReference.LoadSceneAsync(LoadSceneMode.Additive);

            while (!asyncOperation.IsDone)
                await Task.Yield();
        }
    }
}