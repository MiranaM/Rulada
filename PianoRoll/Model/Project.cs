using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{
    public class Project
    {
        public string Title;
        public Dictionary<string, Singer> Singers = new Dictionary<string, Singer>();
        public List<string> SingerNames = new List<string>();
        public List<Track> Tracks = new List<Track>();
        public List<SoundTrack> SoundTracks;
        public static double Tempo = 120;
        public static int BeatPerBar = 4;
        public static int BeatUnit = 4;

        public Project()
        {
            Current = this;
        }

        public static Project Current;

        public Singer AddSinger(string singerDir)
        {
            Singer singer = Singer.Load(singerDir);
            if (!SingerNames.Contains(singer.Name))
            {
                SingerNames.Add(singer.Name);
                Singers[singer.Name] = singer;
                return singer;
            }
            else return Singers[singer.Name];
        }

        public Track AddTrack()
        {
            Track track = new Track();
            track.AddPart();
            Tracks.Add(track);
            return track;
        }

        public Track AddTrack(Part part)
        {
            Track track = new Track();
            track.AddPart(part);
            part.Track = track;
            Tracks.Add(track);
            return track;
        }

        public SoundTrack AddSoundTrack()
        {
            SoundTrack track = new SoundTrack();
            track.AddPart();
            SoundTracks.Add(track);
            return track;
        }

        public SoundTrack AddSoundTrack(SoundPart part)
        {
            SoundTrack track = new SoundTrack();
            track.AddPart(part);
            part.SoundTrack = track;
            SoundTracks.Add(track);
            return track;
        }
    }
}
