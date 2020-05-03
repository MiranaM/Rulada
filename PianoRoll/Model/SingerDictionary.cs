using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PianoRoll.Model
{
    public class SingerDictionary
    {
        private static bool Enabled;
        private static Dictionary<string, string> Dict;
        private const char Separator = '\t';

        public SingerDictionary(Singer singer)
        {
            if (!singer.IsEnabled) return;
            if (singer.VoicebankType == null) return;
            var dir = Path.Combine(Settings.Local, "Dict", singer.VoicebankType + ".dict");
            if (!File.Exists(dir)) return;
            Open(dir);
        }

        private void Open(string dir)
        {
            Dict = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(dir, Encoding.UTF8))
            {
                if (line == "" || !line.Contains(Separator.ToString()))
                    continue;
                var lyric = line.Split(Separator)[0];
                var value = line.Split(Separator)[1];
                Dict[lyric] = value;
            }

            Enabled = true;
        }

        public string Process(string lyric)
        {
            if (Enabled)
            {
                if (!Dict.ContainsKey(lyric))
                    return SeparatedProcess(lyric);
                return Dict[lyric];
            }

            return lyric;
        }

        public string SeparatedProcess(string lyric)
        {
            var phonemes = new List<string>();
            for (var i = 0; i < lyric.Length;)
            {
                var j = lyric.Length - i;
                while (!Dict.ContainsKey(lyric.Substring(i, j)))
                {
                    j--;
                    if (j == 0) return lyric;
                }

                var phs = Dict[lyric.Substring(i, j)];
                phonemes.Add(phs);
                i += j;
            }

            return string.Join(" ", phonemes);
        }
    }
}