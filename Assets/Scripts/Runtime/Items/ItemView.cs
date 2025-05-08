using UnityEngine;
using UnityEngine.UI;

namespace Wigro.Runtime
{
    [DisallowMultipleComponent]
    public sealed class ItemView : MonoBehaviour
    {
        [field: SerializeField] public Image Icon { get; private set; }
    }
}