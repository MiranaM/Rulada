using PianoRoll.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{

    public class Track
    {
        public List<Part> Parts = new List<Part>();
        public TrackControll TrackControll;
        public double Pan;
        public double Volume;
        public Singer Singer;

        public Track()
        {
            Singer = Project.Current.AddSinger(Settings.DefaultVoicebank);
            Singer.Load();
        }

        public Track(string singerDir)
        {
            Singer = Project.Current.AddSinger(singerDir);
            Singer.Load();
        }

        public Part AddPart()
        {
            Part part = new Part();
            Parts.Add(part);
            return part;
        }


        public void AddPart(Part part)
        {
            Parts.Add(part);
            part.Track = this;
        }
    }

}
