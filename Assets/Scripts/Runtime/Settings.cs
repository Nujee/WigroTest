using UnityEngine;

namespace Wigro.Runtime
{
    [System.Serializable]
    public sealed class Settings : ScriptableObject
    {
        [SerializeField, HideInInspector] private Object _folder;
        [SerializeField, HideInInspector] private int _amount;
        [SerializeField, HideInInspector] private int _flags;

        public Object Folder => _folder;
        public int Amount => _amount;
        public bool OpenAnimated { get => (_flags & (int)InventoryFlags.OpenAnimated) != 0; }
        public bool CloseAnimated { get => (_flags & (int)InventoryFlags.CloseAnimated) != 0; }
        public bool ShowInfo { get => (_flags & (int)InventoryFlags.ShowInfo) != 0; }
    }
}
