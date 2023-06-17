using LibBee.Annotations;

namespace XMLBee
{
    public static class XmlActions
    {
        /* The exact actions are omitted. We focus on whether they can be synthesized. */

        [Action]
        public static void DeleteElem(ElementEntity elem) { }

        [Action]
        public static void SelectElem(ElementEntity elem) { }

        [Action]
        public static void ModifyText(ElementEntity elem, string text) { }

        [Action]
        public static void ModifyAttr(AttributeEntity attr, string val) { }

        [Action]
        public static void ModifyTag(ElementEntity elem, string tag) { }

        [Action]
        public static void AddElem(ElementEntity elem, string tag, string text) { }

        [Action]
        public static void AddElemAbove(ElementEntity elem, string tag, string text) { }

        [Action]
        public static void AddAttr(ElementEntity elem, string key, string val) { }

        [Action]
        public static void Wrap(ElementEntity elem, string tag) { }

        [Action]
        public static void MoveBelow(ElementEntity elem, ElementEntity other) { }

        [Action]
        public static void AppendChild(ElementEntity elem, ElementEntity other) { }
    }
}
