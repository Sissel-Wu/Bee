namespace LibProse.hades
{
    public abstract class HDTNode
    {
        public object Key { get; init; }
        
        protected HDTNode() { }

        protected HDTNode(object key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        public override bool Equals(object other)
        {
            return Key.Equals((other as HDTNode)?.Key);
        }

        public abstract bool AllEquals(object other);

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}