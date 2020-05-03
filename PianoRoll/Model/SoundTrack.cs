using System.Collections.Generic;

namespace PianoRoll.Model
{
    public struct PartTransform
    {
        private double X; // translation in track
        private double Offset; // translation in part
        private double Length; // change length
    }

    public class SoundTrack
    {
        public List<SoundPart> Parts;
        public double Pan;
        public double Volume;

        public SoundPart AddPart()
        {
            var part = new SoundPart();
            Parts.Add(part);
            part.SoundTrack = this;
            return part;
        }

        public void AddPart(SoundPart part)
        {
            Parts.Add(part);
            part.SoundTrack = this;
        }
    }
}