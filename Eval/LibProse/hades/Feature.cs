namespace LibProse.hades
{
    public interface IFeature<T, in TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public object EvaluateFeature(TP path);
    }
    
    public delegate object Field<in TA>(TA path);
    public static class FieldFeature<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        private class GetField : IFeature<T, TP>
        {
            private readonly Field<TP> field;
            private readonly string name;

            public GetField(Field<TP> field, string name)
            {
                this.field = field;
                this.name = name;
            }

            public object EvaluateFeature(TP path)
            {
                return field.Invoke(path);
            }
            
            public override string ToString()
            {
                return name;
            }
        }
        
        public static IFeature<T, TP> Of(Field<TP> field, string name)
        {
            return new GetField(field, name);
        }
    }
}