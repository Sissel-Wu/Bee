using LibBee.Annotations;

namespace SprBee
{
    public class CellEntity<TR, TC, TM>
    {
        private readonly int row;
        private readonly int col;
        private int readOrd;
        private readonly TR rowHead;
        private readonly TC colHead;
        private readonly TM content;

        public CellEntity(int row, int col, TR rowHead, TC colHead, TM content)
        {
            this.row = row;
            this.col = col;
            this.rowHead = rowHead;
            this.colHead = colHead;
            this.content = content;
        }

        public void SetReadOrder(int ord)
        {
            readOrd = ord;
        }

        [Field]
        public int Row()
        {
            return row;
        }

        [Field]
        public int Col()
        {
            return col;
        }

        [Field]
        public TR RowHead()
        {
            return rowHead;
        }

        [Field]
        public TC ColHead()
        {
            return colHead;
        }

        [Field]
        public TM Content()
        {
            return content;
        }

        [Field]
        public int ReadOrd()
        {
            return readOrd;
        }
    }

    public class RowEntity<TM>
    {
        private readonly int row;
        private readonly TM[] array;

        public RowEntity(int row, TM[] array)
        {
            this.row = row;
            this.array = array;
        }

        [Field]
        public int Row()
        {
            return row;
        }

        [FixedSizeFields]
        public TM[] Cols()
        {
            return array;
        }
    }

    public static class SprOp
    {
        /* The exact actions are omitted. We focus on whether they can be synthesized. */
        [Action]
        public static void Fill(string content, int row, int col) { }
    }
}
