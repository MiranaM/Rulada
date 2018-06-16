using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{
    public struct PartTransform
    {
        double X; // translation in track
        double Offset; // translation in part
        double Length; // change length
    }

    class SoundTrack
    {
        public SoundPart[] SoundParts;
        public double Pan;
        public double Volume;
    }
}
