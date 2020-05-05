using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model.Pitch
{
    public struct PitchInfo
    {
        public PitchPoint[] PitchPoints;

        // pitch start tick
        public int Start;

        // pitch end tick
        public int End;
    }
}
