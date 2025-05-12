using System;

namespace Wigro.Runtime
{
    public sealed class SlotModel
    {
        public int Id { get; private set; }
        public ItemModel AttachedItem { get; private set; }
        public bool IsEmpty => (AttachedItem == null);

        public event Action<int> OnItemSet = delegate { };

        public SlotModel(int id) => Id = id; 

        public void SetItem(ItemModel newItem)
        {
            AttachedItem = newItem;
            OnItemSet(Id);
        }

        public void Clear() => SetItem(null);
    }
}