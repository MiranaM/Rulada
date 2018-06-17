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

    public class SoundTrack
    {
        public List<SoundPart> Parts;
        public double Pan;
        public double Volume;


        public SoundPart AddPart()
        {
            SoundPart part = new SoundPart();
            Parts.Add(part);
            return part;
        }

        public void AddPart(SoundPart part)
        {
            Parts.Add(part);
        }
    }
}
