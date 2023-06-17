namespace LibProse.hades
{
    public interface IPred<T, in TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public bool EvaluatePred(TP path);
    }
    
    public class TruePred<T, TP> : IPred<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public bool EvaluatePred(TP path)
        {
            return true;
        }
        
        public override string ToString()
        {
            return "true";
        }
    }
    
    public class FalsePred<T, TP> : IPred<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public bool EvaluatePred(TP path)
        {
            return false;
        }
        
        public override string ToString()
        {
            return "false";
        }
    }
    
    public class FeatureEquals<T, TP>: IPred<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        private readonly IFeature<T, TP> feature;
        private readonly object value;
        
        public FeatureEquals(IFeature<T, TP> feature, object value)
        {
            this.feature = feature;
            this.value = value;
        }
        
        public bool EvaluatePred(TP path)
        {
            return feature.EvaluateFeature(path).Equals(value);
        }

        public override string ToString()
        {
            return $"{feature} == {value}";
        }
    }
    
    public class AndPred<T, TP>: IPred<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public IPred<T, TP> left;
        public IPred<T, TP> right;

        public AndPred(IPred<T, TP> left, IPred<T, TP> right)
        {
            this.left = left;
            this.right = right;
        }

        public bool EvaluatePred(TP path)
        {
            return left.EvaluatePred(path) && right.EvaluatePred(path);
        }

        public override string ToString()
        {
            return "(" + left + " && " + right + ")";
        }
    }
    
    public class OrPred<T, TP>: IPred<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public IPred<T, TP> left;
        public IPred<T, TP> right;

        public OrPred(IPred<T, TP> left, IPred<T, TP> right)
        {
            this.left = left;
            this.right = right;
        }

        public bool EvaluatePred(TP path)
        {
            return left.EvaluatePred(path) || right.EvaluatePred(path);
        }
        
        public override string ToString()
        {
            return "(" + left + " || " + right + ")";
        }
    }
    
    public class NotPred<T, TP>: IPred<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public IPred<T, TP> origin;

        public NotPred(IPred<T, TP> origin)
        {
            this.origin = origin;
        }

        public bool EvaluatePred(TP path)
        {
            return origin.EvaluatePred(path);
        }
        
        public override string ToString()
        {
            return "!(" + origin + ")";
        }
    }
}