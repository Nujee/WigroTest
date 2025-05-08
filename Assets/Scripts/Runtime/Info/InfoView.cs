using TMPro;
using UnityEngine;

namespace Wigro.Runtime
{
    public sealed class InfoView : MonoBehaviour
    {
        [field: SerializeField] public RectTransform MainPanel { get; private set; }
        [field: SerializeField] public TMP_Text IDText { get; private set; }
        [field: SerializeField] public TMP_Text RarityText { get; private set; }

        private Slot[] _slots;

        public void Init(bool showInfo, Slot[] slots)
        {
            _slots = slots;

            if (!showInfo)
                return;

            foreach (var slot in _slots)
            {
                //slot.OnClicked += UpdateInfo;
                //slot.OnDragBegun += Close;
            }
        }

        private void OnDestroy()
        {
            //foreach (var slot in _slots)
            //{
            //    slot.OnClicked -= UpdateInfo;
            //}
        }

        private void UpdateInfo(Item item)
        {
            MainPanel.gameObject.SetActive(true);

            IDText.text = item.Data.Id.ToString();
            RarityText.text = item.Data.Rarity.ToString();
        }

        private void Close(Item _) => Close();

        private void Close()
        {
            MainPanel.gameObject.SetActive(false);
        }
    }
}