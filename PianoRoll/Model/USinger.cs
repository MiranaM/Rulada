using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace PianoRoll.Model
{

    public class USinger
    {
        public static string Name { get; set; }
        public static string Author { get; set; }
        public static string Image { get; set; }
        public static string Sample { get; set; }
        public static string UPath { get; set; }
        public static string Readme { get; set; }
        public static List<string> Paths { get; set; }
        public static List<UOto> Otos { get; set; }
        public static bool isEnabled { get; set; }
        
        public USinger() { }

        public static void Load(string dir)
        {
            UPath = dir.Replace("%VOICE%", Settings.VoiceDir);
            isEnabled = File.Exists(Path.Combine(UPath, "oto.ini"));
            if (!isEnabled) return;
            // Обработать отсутствие банка
            Paths = Directory.EnumerateDirectories(UPath).ToList();
            Paths.Add("");
            CharLoad();
            OtoLoad();
            NoteOtoRefresh();
        }

        private static void CharLoad()
        {
            string charfile = Path.Combine(UPath, "character.txt");
            if (File.Exists(charfile))
            {
                string[] charlines = File.ReadAllLines(charfile);
                foreach (string line in charlines)
                {
                    if (line.StartsWith("author=")) Author = line.Substring("author=".Length);
                    if (line.StartsWith("image=")) Image = line.Substring("image=".Length);
                    if (line.StartsWith("name=")) Name = line.Substring("name=".Length);
                    if (line.StartsWith("sample=")) Sample = line.Substring("sample=".Length);
                }
            }
        }

        private static void PrefixMapLoad()
        {

        }

        private static void OtoLoad()
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
                        string pattern = "(.*)=(.*),(.*),(.*),(.*),(.*),(.*)";
                        var arr = Regex.Split(line, pattern);
                        double temp;
                        UOto oto = new UOto()
                        {
                            File = arr[1],
                            Alias = arr[2],
                            Offset = double.TryParse(arr[3], out temp) ? temp : 0,
                            Consonant = double.TryParse(arr[4], out temp) ? temp : 0,
                            Cutoff = double.TryParse(arr[5], out temp) ? temp : 0,
                            Preutter = double.TryParse(arr[6], out temp) ? temp : 0,
                            Overlap = double.TryParse(arr[7], out temp) ? temp : 0,
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

        public static void NoteOtoRefresh()
        {
            foreach (UNote note in Ust.NotesList)
            {
                if (note.IsRest) continue;
                note.Oto = FindOto(note.Lyric);
                if (note.Oto != null) note.HasOto = true;
            }
        }

        public static UOto FindOto(string lyric)
        {
            if (isEnabled)
            {

                foreach (UOto uOto in Otos)
                {
                    if (uOto.Alias == lyric)
                    {
                        return uOto;
                    }
                }
                return null;

            }
            return null;
        }
    }
}
