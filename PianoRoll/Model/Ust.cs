using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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

    internal class Number
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
            return int.TryParse(number.Substring(2, 4), out var i);
        }
    }

    internal class Ust
    {
        public static Part Import(string dir, bool importProject = true)
        {
            var part = Import(dir, out var tempo, out var singerDir);
            return part;
        }

        public static Part Import(string dir, out double tempo, out string singerDir)
        {
            var track = Project.Current.AddTrack();
            var part = track.AddPart();
            var lines = File.ReadAllLines(dir);
            double version;
            tempo = -1;
            singerDir = "";
            long absoluteTime = 0;

            var i = 0;
            if (lines[0] == Number.Version)
            {
                version = 1.2;
                i++;
                i++;
            }

            if (lines[i] != Number.Setting)
                throw new Exception("Error UST reading");
            i++;

            while (i < lines.Length && !Number.IsNote(lines[i]))
            {
                if (lines[i].StartsWith("UstVersion="))
                    if (double.TryParse(lines[i].Substring("UstVersion=".Length), out var v))
                        version = v;
                if (lines[i].StartsWith("Tempo="))
                    if (double.TryParse(lines[i].Substring("Tempo=".Length), out var t))
                        tempo = t;
                if (lines[i].StartsWith("VoiceDir=")) singerDir = lines[i].Substring("VoiceDir=".Length);
                i++;
            }

            part.Notes = new List<Note>();
            while (i + 1 < lines.Length)
            {
                var note = new Note(part);
                // skip number
                i++;
                var pitchData = new USTPitchData(true);
                while (!Number.IsNote(lines[i]) && lines[i] != Number.TrackEnd)
                {
                    var line = lines[i];
                    var parameter = line.Split(new[] {'='}, 2)[0];
                    var value = line.Split(new[] {'='}, 2)[1];

                    switch (parameter)
                    {
                        case "Lyric":
                            note.Lyric = value;
                            break;
                        case "Length":
                            note.Length = double.Parse(value, new CultureInfo("ja-JP").NumberFormat);
                            break;
                        case "STP":
                            note.STP = double.Parse(value, new CultureInfo("ja-JP").NumberFormat);
                            break;
                        case "NoteNum":
                            note.NoteNum = int.Parse(value, new CultureInfo("ja-JP").NumberFormat) - 12;
                            break;
                        case "Envelope":
                            note.Envelope = value;
                            break;
                        case "Velocity":
                            note.Velocity = int.Parse(value, new CultureInfo("ja-JP").NumberFormat);
                            break;
                        case "Modulation":
                            note.Modulation = int.Parse(value, new CultureInfo("ja-JP").NumberFormat);
                            break;
                        case "Intensity":
                            note.Intensity = int.Parse(value, new CultureInfo("ja-JP").NumberFormat);
                            break;
                        case "Flags":
                            note.Flags = value;
                            break;
                        case "VBR":
                            note.Vibrato = value;
                            break;
                        case "PBS":
                            pitchData.PBS = value;
                            break;
                        case "PBW":
                            pitchData.PBW = value;
                            break;
                        case "PBY":
                            pitchData.PBY = value;
                            break;
                        case "PBM":
                            pitchData.PBM = value;
                            break;
                    }

                    i++;
                    if (i == lines.Length) break;
                }

                note.AbsoluteTime = absoluteTime;
                absoluteTime += (long) note.Length;
                PitchFromUst(pitchData, ref note);
                if (note.Lyric.Trim(' ') != "R" && note.Lyric.Trim(' ') != "") part.Notes.Add(note);
            }

            return part;
        }

        public static void PitchFromUst(USTPitchData data, ref Note note)
        {
            if (data.PBS == "")
            {
                data.PBS = "-25";
                data.PBS = "50";
            }

            var pbs = "";
            note.PitchBend = new PitchBendExpression();
            var pts = note.PitchBend.Data as List<PitchPoint>;
            pts.Clear();
            pbs = data.PBS;
            // PBS
            if (pbs.Contains(';'))
            {
                var v1 = double.Parse(pbs.Split(';')[0], new CultureInfo("ja-JP"));
                var v2 = double.Parse(pbs.Split(';')[1], new CultureInfo("ja-JP"));
                pts.Add(new PitchPoint(v1, v2));
                note.PitchBend.SnapFirst = false;
            }
            else
            {
                pts.Add(new PitchPoint(double.Parse(pbs), 0));
                note.PitchBend.SnapFirst = true;
            }

            var x = pts.First().X;
            if (data.PBW != "")
            {
                var w = data.PBW.Split(',');
                string[] y = null;
                if (w.Count() > 1) y = data.PBY.Split(',');
                for (var l = 0; l < w.Count() - 1; l++)
                {
                    x += w[l] == "" ? 0 : float.Parse(w[l]);
                    pts.Add(new PitchPoint(x, y[l] == "" ? 0 : double.Parse(y[l])));
                }

                pts.Add(new PitchPoint(x + double.Parse(w[w.Count() - 1]), 0));

                if (data.PBM != "")
                {
                    var m = data.PBM.Split(',');
                    for (var l = 0; l < m.Count() - 1; l++)
                        pts[l].Shape = m[l] == "r" ? PitchPointShape.o :
                            m[l] == "s" ? PitchPointShape.l :
                            m[l] == "j" ? PitchPointShape.l : PitchPointShape.io;
                }
            }
        }
    }
}