using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model.Pitch
{
    public class ExpPoint : IComparable<ExpPoint>
    {
        public double X;
        public double Y;

        public int CompareTo(ExpPoint other)
        {
            if (X > other.X)
                return 1;
            if (X == other.X)
                return 0;
            return -1;
        }

        public ExpPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public ExpPoint Clone()
        {
            return new ExpPoint(X, Y);
        }
    }
}
