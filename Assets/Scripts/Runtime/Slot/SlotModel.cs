using System;

namespace Wigro.Runtime
{
    public sealed class SlotModel
    {
        public int Id { get; private set; }
        public ItemModel AttachedItem { get; private set; }
        public bool IsEmpty => (AttachedItem == null);

        public event Action<SlotModel> OnItemSet = delegate { };

        public SlotModel(int id) => Id = id; 

        public void SetItem(ItemModel newItem)
        {
            AttachedItem = newItem;
            OnItemSet(this);
        }

        public void Clear() => SetItem(null);
    }
}