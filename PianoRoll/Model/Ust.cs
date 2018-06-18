using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace PianoRoll.Model
{
    public struct USTPitchData
    {
        public string PBS;
        public string PBW;
        public string PBY;
        public string PBM;

        public USTPitchData(bool Empty = false)
        {
            PBS = "";
            PBW = "";
            PBY = "";
            PBM = "";
        }
    }

    class Number
    {
        public const string Next = "[#NEXT]";
        public const string Prev = "[#PREV]";
        public const string Insert = "[#INSERT]";
        public const string Delete = "[#DELETE]";
        public const string Version = "[#VERSION]";
        public const string Setting = "[#SETTING]";
        public const string TrackEnd = "[#TRACKEND]";

        public static bool IsNote(string number)
        {
            if (number.Length < 6) return false;
            if (number == Next) return true;
            if (number == Prev) return true;
            return int.TryParse(number.Substring(2, 4), out int i);
        }
    }

    class Ust
    {
        public static Part Import(string dir, bool importProject = true)
        {
            Part part = Import(dir, out double tempo, out string singerDir);
            return part;
        }

        public static Part Import(string dir, out double tempo, out string singerDir)
        {

            Track track = Project.Current.AddTrack();
            Part part = track.AddPart();
            string[] lines = File.ReadAllLines(dir);
            double version;
            tempo = -1;
            singerDir = "";
            long absoluteTime = 0;

            int i = 0;
            if (lines[0] == Number.Version)
            {
                version = 1.2;
                i++;
                i++;
            }
            if (lines[i] != Number.Setting) throw new Exception("Error UST reading");
            else i++;

            while (i < lines.Length && !Number.IsNote(lines[i]))
            {
                if (lines[i].StartsWith("UstVersion="))
                    if (double.TryParse(lines[i].Substring("UstVersion=".Length), out double v))
                        version = v;
                if (lines[i].StartsWith("Tempo="))
                    if (double.TryParse(lines[i].Substring("Tempo=".Length), out double t))
                        tempo = t;
                if (lines[i].StartsWith("VoiceDir="))
                    singerDir = lines[i].Substring("VoiceDir=".Length);
                i++;
            }

            part.Notes = new List<Note>();
            while (i + 1 < lines.Length)
            {
                Note note = new Note(part);
                // skip number
                i++;
                USTPitchData pitchData = new USTPitchData(true);
                while (!Number.IsNote(lines[i]) && lines[i] != Number.TrackEnd)
                {
                    string line = lines[i];
                    var parameter = line.Split(new char[] { '=' }, count: 2)[0];
                    var value = line.Split(new char[] { '=' }, count: 2)[1];

                    switch (parameter)
                    {
                        case "Lyric": note.Lyric = value; break;
                        case "Length": note.Length = double.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                        case "STP": note.STP = double.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                        case "NoteNum": note.NoteNum = int.Parse(value, new CultureInfo("ja-JP").NumberFormat) - 24; break;
                        case "Envelope": note.Envelope = value; break;
                        case "Velocity": note.Velocity = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                        case "Modulation": note.Modulation = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                        case "Intensity": note.Intensity = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                        case "Flags": note.Flags = value; break;
                        case "VBR": note.Vibrato = value; break;
                        case "PBS": pitchData.PBS = value; break;
                        case "PBW": pitchData.PBW = value; break;
                        case "PBY": pitchData.PBY = value; break;
                        case "PBM": pitchData.PBM = value; break;
                    }
                    i++;
                    if (i == lines.Length) break;
                }
                note.AbsoluteTime = absoluteTime;
                absoluteTime += (long)note.Length;
                Pitch.PitchFromUst(pitchData, ref note);
                part.Notes.Add(note);
            }
            return part;
        }
    }
}
