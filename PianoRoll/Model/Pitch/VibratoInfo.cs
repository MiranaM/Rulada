using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model.Pitch
{
    public struct VibratoInfo
    {
        // vibrato start ms
        public double Start;

        // vibrato end ms
        public double End;

        // note length tick
        public long Length;

        // vibrato
        public VibratoExpression Vibrato;
    }
}
