using Wigro.Runtime;

public sealed class Inventory
{
    private Slot[] _slots;
    private InventoryView _view;

    public Inventory Init(Settings settings, InventoryView view)
    {
        _view = view;

        // var itemsBySlotIndex = TransformToInnerDataBase(outerDataBase);

        var slotsAmount = settings.Amount;
        _slots = new Slot[slotsAmount];
        var slotPool = new GenericPool<Slot>(_view.SlotPrefab, slotsAmount, _view.SlotsParent);

        for (int i = 0; i < slotsAmount; i++)
        {
            //_slots.
            var slot = slotPool.Get();
            //slot.Init(itemsBySlotIndex[i]);
            //_slots[i] = slot;
        }

         return this;
    }
}