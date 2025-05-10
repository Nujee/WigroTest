using System;

namespace Wigro.Runtime
{
    public sealed class ItemModel
    {
        public ItemData Data { get; private set; }

        public event Action<ItemModel> OnRemove = delegate { };

        public ItemModel(ItemData data) => Data = data;

        public void Remove() => OnRemove(this);
    }
}