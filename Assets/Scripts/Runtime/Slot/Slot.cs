using System;

namespace Wigro.Runtime
{
    public sealed class Slot
    {
        public Item AttachedItem { get; private set; }
        public bool IsEmpty => (AttachedItem == null);
        public Action<Item> OnItemSet = delegate { };

        public void SetItem(Item item)
        {
            OnItemSet(item);

            AttachedItem = item 
                ?? throw new ArgumentNullException(nameof(item), "Use Clear() instead of SetItem(null).");
        }

        public void Clear() => AttachedItem = null;
    }
}