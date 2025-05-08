using System;

namespace Wigro.Runtime
{
    public sealed class Slot
    {
        public Item Item { get; private set; }
        public bool IsEmpty => (Item == null);

        public void SetItem(Item item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item), "Use Clear() instead of SetItem(null).");
        }

        public void Clear() => Item = null;
    }
}