using System.Collections.Generic;
using System.IO;
using PianoRoll.Control;

namespace PianoRoll.Model
{
    public class Project
    {
        public string Title;

        public List<Track> Tracks = new List<Track>();
        public List<SoundTrack> SoundTracks;
        public string Dir;
        public bool IsNew = true;
        public Singer DefaultSinger;

        //public int UnitPerBar => BeatPerBar * BeatUnit;

        public Project(Singer defaultSinger)
        {
            DefaultSinger = defaultSinger;
        }

        public Track AddTrack()
        {
            var track = new Track(DefaultSinger);
            Tracks.Add(track);
            return track;
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
        }

        public SoundTrack AddSoundTrack()
        {
            var track = new SoundTrack();
            SoundTracks.Add(track);
            return track;
        }

        public void AddSoundTrack(SoundTrack track)
        {
            SoundTracks.Add(track);
        }




    }
}