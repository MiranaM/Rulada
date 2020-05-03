using System.Collections.Generic;
using PianoRoll.Control;

namespace PianoRoll.Model
{
    public class Project
    {
        public string Title;
        public Dictionary<string, Singer> Singers = new Dictionary<string, Singer>();
        public List<string> SingerNames = new List<string>();
        public List<Track> Tracks = new List<Track>();
        public List<SoundTrack> SoundTracks;
        public string Dir;
        public bool IsNew = true;

        public static double Tempo = 120;
        public static int BeatPerBar = 4;
        public static int BeatUnit = 32;

        public static int MinNoteLengthTick => Settings.Resolution / BeatUnit;

        public static double MinNoteLengthX => Settings.Resolution / BeatUnit * PartEditor.xScale;

        public static int UnitPerBar => BeatPerBar * BeatUnit;

        public Project()
        {
            Current = this;
        }

        public static Project Current;

        /// <summary>
        ///     Add singer by directory or name
        /// </summary>
        /// <param name="singerDir"></param>
        /// <returns></returns>
        public Singer AddSinger(string singerDir)
        {
            var singer = Singer.Find(singerDir);
            if (singer != null) return singer;
            singer = Singer.Load(singerDir);
            if (!SingerNames.Contains(singer.Name))
            {
                SingerNames.Add(singer.Name);
                Singers[singer.Name] = singer;
                return singer;
            }

            return Singers[singer.Name];
        }

        public Track AddTrack()
        {
            var track = new Track();
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