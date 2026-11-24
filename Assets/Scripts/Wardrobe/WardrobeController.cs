namespace FireSlop.Wardrobe
{
    public sealed class WardrobeController
    {
        private readonly WardrobeModel _model;

        public WardrobeController(WardrobeModel model)
        {
            _model = model;
        }

        public void Open()
        {
            _model.Show(true);
        }

        public void Cancel()
        {
            _model.Show(false);
        }

        public void Confirm()
        {
            _model.Commit();
        }

        public void Filter(WardrobeCategory category)
        {
            _model.FilterBy(category);
        }

        public void Select(WardrobeItem item)
        {
            _model.Select(item);
        }

        public void TurnPage(int delta, int capacity)
        {
            _model.TurnPage(delta, capacity);
        }
    }
}
