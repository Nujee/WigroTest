using TMPro;
using UnityEngine;

namespace Wigro.Runtime
{
    public sealed class InfoView : MonoBehaviour
    {
        [field: SerializeField] public RectTransform MainPanel { get; private set; }
        [field: SerializeField] public TMP_Text IDText { get; private set; }
        [field: SerializeField] public TMP_Text RarityText { get; private set; }

        public void UpdateInfo(string itemId, int rarity)
        {
            IDText.text = itemId;
            RarityText.text = ((ItemRarity)rarity).ToString() + $" ({rarity})"; // e.g. "Epic (2)"
        }

        public void Close() => ToggleState(false);

        public void Show() => ToggleState(true);

        private void ToggleState(bool isActive) => MainPanel.gameObject.SetActive(isActive);
    }
}