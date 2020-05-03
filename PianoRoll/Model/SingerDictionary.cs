using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PianoRoll.Model
{
    public class SingerDictionary
    {


        public SingerDictionary(Singer singer)
        {
            if (!singer.IsEnabled)
                return;
            if (singer.VoicebankType == null)
                return;
            var dir = Path.Combine(Settings.Local, "Dict", singer.VoicebankType + ".dict");
            if (!File.Exists(dir))
                return;
            Open(dir);
        }

        public string Process(string lyric)
        {
            if (enabled)
            {
                if (!dict.ContainsKey(lyric))
                    return SeparatedProcess(lyric);
                return dict[lyric];
            }

            return lyric;
        }

        public string SeparatedProcess(string lyric)
        {
            var phonemes = new List<string>();
            for (var i = 0; i < lyric.Length;)
            {
                var j = lyric.Length - i;
                while (!dict.ContainsKey(lyric.Substring(i, j)))
                {
                    j--;
                    if (j == 0) return lyric;
                }

                var phs = dict[lyric.Substring(i, j)];
                phonemes.Add(phs);
                i += j;
            }

            return string.Join(" ", phonemes);
        }

        #region private

        private const char SEPARATOR = '\t';

        private static bool enabled;
        private static Dictionary<string, string> dict;

        private void Open(string dir)
        {
            dict = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(dir, Encoding.UTF8))
            {
                if (line == "" || !line.Contains(SEPARATOR.ToString()))
                    continue;
                var lyric = line.Split(SEPARATOR)[0];
                var value = line.Split(SEPARATOR)[1];
                dict[lyric] = value;
            }

            enabled = true;
        }

        #endregion
    }
}