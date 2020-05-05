using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PianoRoll.Util;

namespace PianoRoll.Model
{
    public class Singer
    {
        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Image { get; private set; }
        public string Sample { get; private set; }
        public string Dir { get; }
        public string VoicebankType { get; private set; }
        public string Readme { get; private set; }
        public List<string> Subs { get; private set; }
        public List<Oto> Otos { get; private set; }
        public bool IsEnabled { get; private set; }
        public SingerDictionary SingerDictionary;



        public bool IsVowel(string phoneme)
        {
            // TODO: do right way
            return
                phoneme == "a" ||
                phoneme == "e" ||
                phoneme == "i" ||
                phoneme == "o" ||
                phoneme == "u";
        }

        public double GetConsonantLength(string phoneme)
        {
            // probably defines the way we configure utau
            // probably PRE is fixed and OVL is not
            // TODO: do right way
            var value = 50; // oto: ovl 30 pre 40
            if (phoneme == "r" || phoneme == "r'")
                value = 30; // oto: ovl 20 pre 15
            else if (phoneme == "ch"
                || phoneme == "t'"
                || phoneme == "ts"
                || phoneme == "zh"
                || phoneme == "sh"
                || phoneme == "sh'"
                || phoneme == "k'"
            )
                value = 150; // oto: ovl 110 pre 20
            return MusicMath.Current.TickToMillisecond(value);
        }

        public double GetRestLength(string phoneme)
        {
            // TODO: do right way
            return 60;
        }

        public Singer(string dir)
        {
            Dir = dir;
            CheckVoicebank();
            CharLoad();
        }

        private void CheckVoicebank()
        {
            if (!Directory.Exists(Dir))
            {
                IsEnabled = false;
                return;
            }

            Subs = Directory.EnumerateDirectories(Dir).Select(n => Path.GetFileName(n)).ToList();
            Subs.Add("");
            IsEnabled = false;
            foreach (var sub in Subs)
            {
                var subdir = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(subdir))
                {
                    IsEnabled = true;
                    return;
                }
            }

            IsEnabled = false;
        }

        private void CharLoad()
        {
            var charfile = Path.Combine(Dir, "character.txt");
            if (IsEnabled && File.Exists(charfile))
            {
                var charlines = File.ReadAllLines(charfile);
                foreach (var line in charlines)
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
            Otos = new List<Oto>();
            foreach (var sub in Subs)
            {
                var filename = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(filename))
                {
                    var lines = File.ReadAllLines(filename);
                    foreach (var line in lines)
                    {
                        var pattern = "(.*)=(.*),(.*),(.*),(.*),(.*),(.*)";
                        var arr = Regex.Split(line, pattern);
                        double temp;
                        if (arr.Length == 1) continue;
                        var oto = new Oto
                        {
                            File = arr[1],
                            Alias = arr[2],
                            Offset = double.TryParse(arr[3], out temp) ? temp : 0,
                            Consonant = double.TryParse(arr[4], out temp) ? temp : 0,
                            Cutoff = double.TryParse(arr[5], out temp) ? temp : 0,
                            Preutter = double.TryParse(arr[6], out temp) ? temp : 0,
                            Overlap = double.TryParse(arr[7], out temp) ? temp : 0
                        };
                        Otos.Add(oto);
                    }
                }
                else
                {
                    File.Create(filename);
                }
            }

            SingerDictionary = new SingerDictionary(this);
        }

        public Oto FindOto(string lyric)
        {
            if (IsEnabled)
            {
                foreach (var oto in Otos)
                    if (oto.Alias == lyric)
                        return oto;

                return null;
            }

            return null;
        }
    }
}