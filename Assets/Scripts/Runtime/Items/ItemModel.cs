namespace Wigro.Runtime
{
    public sealed class ItemModel
    {
        public ItemData Data { get; private set; }
        public ItemModel(ItemData data) => Data = data;
    }
}