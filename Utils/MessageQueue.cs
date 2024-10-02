namespace Utils
{
    public interface IMessage { }

    public class MessageQueue
    {
        private readonly Dictionary<Type, List<object>> _handlers = new();
        public void Send<T>(T message) where T : IMessage
        {
            if(!_handlers.ContainsKey(typeof(T)))
            {
                return;
            }

            foreach (var handler in _handlers[typeof(T)])
            {
                ((Action<T>)handler)(message);
            }
        }

        public void Register<T>(Action<T> handler) where T : IMessage
        {
            var type = typeof(T);

            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new List<object>();
            }

            _handlers[type].Add(handler);
        }
    }
}
