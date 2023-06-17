using System;

namespace SprProse
{
    public class Coord
    {
        public int X { get; }
        public int Y { get; }
        
        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public static Coord ZeroPoint => new Coord(0, 0);

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj.GetType() == typeof(Coord) && X == ((Coord)obj).X && Y == ((Coord)obj).Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}