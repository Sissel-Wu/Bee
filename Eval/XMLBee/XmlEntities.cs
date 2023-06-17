using LibBee.Annotations;

namespace XMLBee
{
    [Entity]
    public class ElementEntity
    {
        private readonly string id;
        private ElementEntity parent;
        private ElementEntity prev;
        private ElementEntity next;
        private readonly string tag;
        private readonly string text;

        public ElementEntity(string id, string tag, string text)
        {
            this.id = id;
            this.tag = null;
            this.tag = tag;
            next = null;
            this.text = text;
        }

        public void SetParent(ElementEntity parent)
        {
            this.parent = parent;
        }

        public void SetPrev(ElementEntity prev)
        {
            this.prev = prev;
        }

        public void SetNext(ElementEntity next)
        {
            this.next = next;
        }
        
        [IdField]
        public ElementEntity Self()
        {
            return this;
        }

        [IdField]
        public ElementEntity Parent()
        {
            return parent;
        }

        [IdField]
        public ElementEntity Prev()
        {
            return prev;
        }

        [IdField]
        public ElementEntity Next()
        {
            return next;
        }

        [Field]
        public string Tag()
        {
            return tag;
        }

        [Field]
        public string Text()
        {
            return text;
        }

        public override string ToString()
        {
            return id;
        }
    }

    [Entity]
    public class AttributeEntity
    {
        private readonly string id;
        private readonly string key;
        private readonly string val;
        private readonly ElementEntity owner;

        public AttributeEntity(string id, ElementEntity owner, string key, string val)
        {
            this.id = id;
            this.owner = owner;
            this.key = key;
            this.val = val;
        }
            
        [IdField]
        public AttributeEntity Self()
        {
            return this;
        }

        [IdField]
        public ElementEntity Owner()
        {
            return owner;
        }

        [Field]
        public string Key()
        {
            return key;
        }

        [Field]
        public string Val()
        {
            return val;
        }

        public override string ToString()
        {
            return id;
        }
    }
}
