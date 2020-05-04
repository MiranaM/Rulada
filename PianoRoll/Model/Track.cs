using System;
using System.Collections.Generic;
using PianoRoll.Control;

namespace PianoRoll.Model
{
    public class Track
    {
        public List<Part> Parts = new List<Part>();
        public TrackControl TrackControl;
        public double Pan;
        public double Volume;
        public Singer Singer;

        public Track(Singer singer)
        {
            Singer = singer;
            if (!Singer.IsEnabled) 
                throw new Exception("dsdasdf");
        }

        public Part AddPart()
        {
            var part = new Part();
            Parts.Add(part);
            part.Track = this;
            return part;
        }

        public void AddPart(Part part)
        {
            Parts.Add(part);
            part.Track = this;
        }
    }
}