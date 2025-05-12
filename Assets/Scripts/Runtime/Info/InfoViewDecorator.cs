namespace Wigro.Runtime
{
    public sealed class InfoViewDecorator
    {
        private readonly InfoView _wrapped;
        private readonly Settings _settings;

        public InfoViewDecorator(InfoView wrapped, Settings settings)
        {
            _wrapped = wrapped;
            _settings = settings;
        }

        public void UpdateInfo(string itemId, int rarity)
        {
            if (_settings.ShowInfo)
            {
                _wrapped.UpdateInfo(itemId, rarity);
            }
        }

        public void Open()
        {
            if (_settings.ShowInfo)
                _wrapped.Open();
        }

        public void Close()
        {
            if (_settings.ShowInfo)
                _wrapped.Close();
        }
    }
}