namespace Wigro.Runtime
{
    public sealed class Item
    {
        public ItemView View { get; private set; }
        public ItemData Data { get; private set; }

        public Item(ItemView view, ItemData data)
        {
            View = view;
            Data = data;
        }
    }
}