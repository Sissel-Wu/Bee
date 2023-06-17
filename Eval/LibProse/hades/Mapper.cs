namespace LibProse.hades
{
    public interface IMapper<T> where T: HDTNode
    {
        public T Evaluate(T node);
        public bool CanMap(T from, T to);
    }
    
    public class ConstMapper<T> : IMapper<T> where T: HDTNode, new()
    {
        private readonly object key;

        public ConstMapper(object key)
        {
            this.key = key;
        }

        public T Evaluate(T fileNode)
        {
            return new T {Key = key};
        }

        public bool CanMap(T from, T to)
        {
            return to.Key.Equals(key);
        }

        public override string ToString()
        {
            return "Const(" + key + ")";
        }
    }
}