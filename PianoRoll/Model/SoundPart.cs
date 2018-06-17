using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;

namespace PianoRoll.Model
{
    public class SoundPart
    {
        public WaveStream WaveStream;
        public PartTransform PartTransform;
        public SoundTrack SoundTrack;
    }
}
