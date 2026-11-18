namespace FireSlop.Interaction
{
    internal sealed class FireSpreadModel
    {
        private WaterFireModel[] _fires;
        private WaterFireModel _seed;
        private int _burningCount;

        public bool HasBurningCells => _burningCount > 0;

        public void Initialize(int count, int seedIndex, WaterFireModel seed)
        {
            _fires = new WaterFireModel[count];
            _seed = seed;
            _fires[seedIndex] = seed;
            _burningCount = 1;
            seed.Extinguished += OnExtinguished;
        }

        public bool CanSpread(int index, double time)
        {
            return _fires[index]?.CanSpread(time) == true;
        }

        public bool CanIgnite(int index, double time)
        {
            return _fires[index]?.CanIgnite(time) ?? true;
        }

        public WaterFireModel Ignite(int index, double time, float interval)
        {
            var fire = _fires[index];
            if (fire == null)
            {
                fire = _seed.CreateUnlit();
                fire.Extinguished += OnExtinguished;
                _fires[index] = fire;
            }

            fire.Ignite(time, interval);
            _burningCount++;
            return fire;
        }

        private void OnExtinguished()
        {
            _burningCount--;
        }
    }
}
