﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{
    public class SingerDictionary
    {
        static bool Enabled = false;
        static Dictionary<string, string> Dict;

        public SingerDictionary(Singer singer)
        {
            if (!singer.IsEnabled) return;
            if (singer.VoicebankType == null) return;
            string dir = Path.Combine(Settings.Local, "Dict", singer.VoicebankType + ".dict");
            if (!File.Exists(dir)) return;
            Open(dir);
        }

        void Open(string dir)
        {
            Dict = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(dir, Encoding.UTF8))
            {
                if (line == "") continue;
                var lyric = line.Split('=')[0];
                var value = line.Split('=')[1];
                Dict[lyric] = value;
            }
            Enabled = true;
        }

        public string Process(string lyric)
        {
            if (Enabled)
            {
                if (!Dict.ContainsKey(lyric)) return lyric;
                else return Dict[lyric];
            }
            else return lyric;
        }
    }
}