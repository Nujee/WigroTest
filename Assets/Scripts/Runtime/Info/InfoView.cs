using UnityEngine;
using UnityEngine.UI;

public sealed class InfoView : MonoBehaviour
{
    [field: SerializeField] public RectTransform MainPanel { get; private set; }
    [field: SerializeField] public Text Text { get; private set; }
}
