using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace PianoRoll.Model
{

    class USinger
    {
        public string Name { get; set; }
        public string UPath { get; set; }
        public string Readme { get; set; }
        public List<string> Paths { get; set; }
        public List<UOto> Otos { get; set; }

        public USinger(string dir)
        {
            Paths = Directory.EnumerateDirectories(dir).ToList();
            Paths.Add("");
            OtoLoad();
        }

        private void OtoLoad()
        {
            Otos = new List<UOto> { };
            foreach (string path in Paths)
            {
                string filename = Path.Combine(UPath, path, "oto.ini");
                if (File.Exists(filename))
                {
                    string[] lines = File.ReadAllLines(filename);
                    foreach (string line in lines)
                    {
                        var arr = Regex.Split(line, "([*])=(*),(*),(*),(*),(*),(*)");
                        UOto oto = new UOto()
                        {
                            File = arr[0],
                            Alias = arr[1],
                            Offset = double.Parse(arr[2]),
                            Consonant = double.Parse(arr[3]),
                            Cutoff = double.Parse(arr[4]),
                            Preutter = double.Parse(arr[5]),
                            Overlap = double.Parse(arr[6])
                        };
                        Otos.Add(oto);
                    }
                }
                else
                {
                    File.Create(filename);
                }
            }
        }

        public UOto FindOto(string lyric)
        {
            foreach (UOto uOto in Otos)
            {
                if (uOto.Alias == lyric)
                {
                    return uOto;
                }
            }
            return new UOto()
            {
                File = $"{lyric}.wav",
                Alias = lyric,
                Offset = 0,
                Consonant = 0,
                Cutoff = 0,
                Preutter = 0,
                Overlap = 0
            };
        }
    }
}
