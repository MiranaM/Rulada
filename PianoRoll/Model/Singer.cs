using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace PianoRoll.Model
{

    public class Singer
    {
        public static List<Singer> Singers = new List<Singer>();
        public static Dictionary<string, Singer> SingerNames = new Dictionary<string, Singer>();

        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Image { get; private set; }
        public string Sample { get; private set; }
        public string Dir { get; private set; }
        public string VoicebankType { get; private set; }
        public string Readme { get; private set; }
        public List<string> Subs { get; private set; }
        public List<Phoneme> Phonemes { get; private set; }
        public bool IsEnabled { get; private set; }
        public SingerDictionary SingerDictionary;

        public static void FindSingers()
        {
            foreach (string dir in Directory.EnumerateDirectories(Settings.VoicebankDirectory))
            {
                Singer.Load(dir);
            }
        }

        public static Singer Find(string name)
        {
            if (SingerNames.ContainsKey(name))
            {
                return SingerNames[name];
            }
            else return null;
        }

        public static Singer Load(string dir)
        {
            if (dir.StartsWith("%VOICE%"))
            {
                string name = dir.Replace("%VOICE%", "");
                if (SingerNames.ContainsKey(name)) return SingerNames[name];
                dir = Path.Combine(Settings.VoicebankDirectory, name);
            }
            Singer singer = new Singer(dir);
            if (SingerNames.ContainsKey(singer.Name)) return SingerNames[singer.Name];
            else
            {
                singer.Add();
                return singer;
            }
        }
        
        private Singer(string dir)
        {
            Dir = dir;
            CheckVoicebank();
            CharLoad();
        }

        void Add()
        {
            Singers.Add(this);
            SingerNames[Name] = this;
        }

        void CheckVoicebank()
        {
            if (!Directory.Exists(Dir))
            {
                IsEnabled = false;
                return;
            }
            Subs = System.IO.Directory.EnumerateDirectories(Dir).Select(n => Path.GetFileName(n)).ToList();
            Subs.Add("");
            IsEnabled = false;
            foreach (string sub in Subs)
            {
                string subdir = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(subdir))
                {
                    IsEnabled = true;
                    return;
                }
            }
            IsEnabled = false;
            return;
        }

        private void CharLoad()
        {
            string charfile = Path.Combine(Dir, "character.txt");
            if (IsEnabled && File.Exists(charfile))
            {
                string[] charlines = File.ReadAllLines(charfile);
                foreach (string line in charlines)
                {
                    if (line.StartsWith("author=")) Author = line.Substring("author=".Length);
                    if (line.StartsWith("image=")) Image = line.Substring("image=".Length);
                    if (line.StartsWith("name=")) Name = line.Substring("name=".Length);
                    if (line.StartsWith("sample=")) Sample = line.Substring("sample=".Length);
                    if (line.StartsWith("VoicebankType=")) VoicebankType = line.Substring("VoicebankType=".Length);
                    if (line.StartsWith("type=")) VoicebankType = line.Substring("type=".Length);
                }
            }
            if (Name == null) Name = Path.GetFileName(Dir);
        }

        public void Load()
        {
            Phonemes = new List<Phoneme> { };
            foreach (string sub in Subs)
            {
                string filename = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(filename))
                {
                    string[] lines = File.ReadAllLines(filename);
                    foreach (string line in lines)
                    {
                        string pattern = "(.*)=(.*),(.*),(.*),(.*),(.*),(.*)";
                        var arr = Regex.Split(line, pattern);
                        double temp;
                        if (arr.Length == 1) continue;
                        Phoneme phoneme = new Phoneme()
                        {
                            File = arr[1],
                            Alias = arr[2],
                            Offset = double.TryParse(arr[3], out temp) ? temp : 0,
                            Consonant = double.TryParse(arr[4], out temp) ? temp : 0,
                            Cutoff = double.TryParse(arr[5], out temp) ? temp : 0,
                            Preutter = double.TryParse(arr[6], out temp) ? temp : 0,
                            Overlap = double.TryParse(arr[7], out temp) ? temp : 0,
                        };
                        Phonemes.Add(phoneme);
                    }
                }
                else File.Create(filename);
            }
            SingerDictionary = new SingerDictionary(this);
        }

        public Phoneme FindPhoneme(string lyric)
        {
            if (IsEnabled)
            {
                foreach (Phoneme phoneme in Phonemes)
                {
                    if (phoneme.Alias == lyric)
                    {
                        return phoneme;
                    }
                }
                return null;
            }
            return null;
        }
    }
}
