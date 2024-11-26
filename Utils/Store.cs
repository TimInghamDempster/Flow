namespace Utils
{
    public class Store<T>
    {
        private readonly Dictionary<Guid, T> _items = [];
        public Store()
        {
        }

        public T Get(Guid id) => _items[id];

        public void Add(Guid id, T item)
        {
            _items.Add(id, item);
        }

        public void Update(Guid id, T item)
        {
            _items[id] = item;
        }
    }
}
