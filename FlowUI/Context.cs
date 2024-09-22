using FlowCompiler;

namespace FlowUI
{
    public class Context<T>
    {
        private T _current;
        public Context(T initial)
        {
            _current = initial;
        }

        public T Current => _current;

        public event Action? Updated;

        internal void Update(T newContent)
        {
            _current = newContent;
            Updated?.Invoke();
        }
    }
}
