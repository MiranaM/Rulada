using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model.Pitch
{

    public class PitchPoint : ExpPoint
    {
        public PitchPointShape Shape;

        public PitchPoint(double x, double y, PitchPointShape shape = PitchPointShape.io) : base(x, y)
        {
            Shape = shape;
        }

        public new PitchPoint Clone()
        {
            return new PitchPoint(X, Y, Shape);
        }
    }

    public enum PitchPointShape
    {
        /// <summary>
        ///     SineInOut
        /// </summary>
        io,

        /// <summary>
        ///     Linear
        /// </summary>
        l,

        /// <summary>
        ///     SineIn
        /// </summary>
        i,

        /// <summary>
        ///     SineOut
        /// </summary>
        o
    }
}
