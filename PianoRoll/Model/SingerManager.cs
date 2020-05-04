using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{
    public class SingerManager
    {
        public Dictionary<string, Singer> Singers = new Dictionary<string, Singer>();
        public List<string> SingerNames = new List<string>();
        public Singer DefaultSinger;

        public SingerManager()
        {
            InitSingers();
            if (!Singers.ContainsKey(Settings.Current.DefaultVoicebank))
                throw new Exception("нет такого");
            DefaultSinger = Singers[Settings.Current.DefaultVoicebank];
            DefaultSinger.Load();

        }
        public Singer FindSinger(string name)
        {
            if (Singers.ContainsKey(name))
                return Singers[name];
            return null;
        }

        public void LoadSinger(string dir)
        {
            if (dir.StartsWith("%VOICE%"))
            {
                var name = dir.Replace("%VOICE%", "");
                dir = Path.Combine(Settings.Current.VoicebankDirectory, name);
            }

            var singer = new Singer(dir);
            if (Singers.ContainsKey(singer.Name))
                return;

            SingerNames.Add(singer.Name);
            Singers[singer.Name] = singer;
        }

        private void InitSingers()
        {
            var dirs = Directory.EnumerateDirectories(Settings.Current.VoicebankDirectory);

            foreach (var dir in dirs)
                LoadSinger(dir);
        }
    }
}
