using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PianoRoll.Util;
using System.Globalization;

namespace PianoRoll.Model
{
    enum Insert
    {
        Before,
        After
    }

    class Ust
    {
        static public string[] USettingsList =
        {
            "ProjectName",
            "Tempo",
            "Tracks",
            "VoiceDir",
            "CacheDir",
            "OutFile",
            "Tool1",
            "Tool2",
            "Mode2",
            "Flags"
        };
        static public string LOG_Dir = @"log.txt";
        static public string Text;
        static public Dictionary<string, string> uSettings = new Dictionary<string, string> { };
        static public UNote uDefaultNote = new UNote();
        //static public Dictionary<string, UNote> uNotes;
        //static public string[] Numbers;
        static public List<UNote> NotesList = new List<UNote>();
        static public string uVersion;
        static public string VoiceDir;
        static public int NotesCount = 0;
        static public string Flags
        {
            get { return uSettings.ContainsKey("Flags") ? uSettings["Flags"] : ""; }
            set { uSettings["Flags"] = value; }
        }

        static public string Dir;

        public static void Load(string dir)
        {
            Dir = dir;
            SetDefaultNoteSettings();
            NotesList.Clear();
            Open();
        }

        private static void Open()
        {
            Ini ini = new Ini(Dir);
            Read(ini.data, ini.Sections.ToArray());
        }

        private static void Read(dynamic data, string[] sections)
        {
            NotesList.Clear();

            string stage = "Init";
            stage = "Version";
            // Reading string
            if (data.ContainsKey("[#VERSION]")) uVersion = data["[#VERSION]"];
            //else uVersion = data["[#SETTING]"].uVersion;

            // Reading settings
            stage = "Setting";
            data["[#SETTING]"].Keys.CopyTo(USettingsList, 0);
            foreach (string setting in USettingsList)
            {
                stage = $"Setting: {setting}";
                if (data["[#SETTING]"].ContainsKey(setting))
                {
                    uSettings[setting] = data["[#SETTING]"][setting];
                }
            }
            Settings.Tempo = double.Parse((data["[#SETTING]"]["Tempo"]), new CultureInfo("ja-JP"));
            VoiceDir = uSettings["VoiceDir"].Replace("%VOICE%", "");

            // Sections - Version, Settings;
            stage = "Notes count";
            NotesCount = data["SectionsNumber"] - 3;

            long absoluteTime = 0;

            // Reading notes
            stage = "Notes";
            //uNotes = new Dictionary<string, UNote> { };
            //Numbers = new string[NotesCount];
            stage = "Searching first note number";
            int firstNote = -1;
            firstNote = NoteNumber2Number(sections[3]);

            for (int i = 0; i < NotesCount; i++)
            {
                stage = $"Note: {i}";
                UNote note = NoteRead(data, (i + firstNote).ToString(), ref absoluteTime, out string number);
                //uNotes[number] = note;
                //Numbers[i] = number;
                NotesList.Add(note);
            }
        }

        private static UNote NoteRead(dynamic data, string which, ref long absoluteTime, out string number)
        {
            // May be #PREV, #0000 .... #NNNN, #NEXT
            if (int.TryParse(which, out int tempInt)) number = Number2NoteNumber(tempInt);
            else number = $"[#{which}]";

            UNote note = new UNote();
            note.SetDefaultNoteSettings();
            int i = 0;
            Console.WriteLine($"Setting values for note {number}");
            foreach (string parameter in data[number].Keys)
            {
                Console.WriteLine($"\tTrying set parameter  {parameter}");
                var value = data[number][parameter];
                switch(parameter)
                {
                    case "Lyric": note.Lyric = value; break;
                    case "Length": note.Length = double.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                    case "STP": note.STP = double.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                    case "NoteNum": note.NoteNum = int.Parse(value, new CultureInfo("ja-JP").NumberFormat) - 12; break;
                    case "Envelope": note.Envelope = value; break;
                    case "Velocity": note.Velocity = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                    case "Modulation": note.Modulation = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                    case "Intensity": note.Intensity = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); break;
                    case "Flags": note.Flags = value; break;
                    case "VBR": note.Vibrato = value; break;
                }
                i++;
            }
            note.UNumber = number;
            note.AbsoluteTime = absoluteTime;
            absoluteTime += (long)note.Length;
            UPitch.PitchFromUst(data[number], ref note);
            return note;
        }

        public static bool IsTempUst(string Dir)
        {
            string filename = Dir.Split('\\').Last();
            return filename.StartsWith("tmp") && filename.EndsWith(".tmp");
        }

        public static void SetDefaultNoteSettings()
        {
            // We will apply this to "r" note which we won't consider Rest
            uDefaultNote.Intensity = 100;
            uDefaultNote.Modulation = 0;
            uDefaultNote.Envelope = "0,21,35,0,100,100,0,%,0";
            uDefaultNote.Vibrato = new VibratoExpression();
        }


        public static void SetLyric(string[] lyric, bool skipRest = true)
        {
            int i = 0;
            foreach (UNote note in NotesList)
            {
                if (note.IsRest) continue;
                note.Lyric = lyric[i];
                i++;
            }
        }


        public static int GetLength(string lyric)
        {
            // it supposed to be count from oto.ini
            return 120;
        }

        public static string UpgradeNumber(string initNumber, int i, Insert insert = Insert.Before)
        {
            string newNumber;
            int initI = int.Parse(initNumber.Substring(2, 4));
            int newI;
            if (insert == Insert.Before)
            {
                newI = initI + i;
            }
            else
            {
                newI = initI - i;
            }
            newNumber = $"[#{newI.ToString().PadLeft(4, '0')}]";
            return newNumber;
        }

        public static string UpgradeNumber(string initNumber, int i, int secondI, Insert insert = Insert.Before)
        {
            string newNumber;
            int initI = int.Parse(initNumber.Substring(2, 4));
            int newI;
            if (insert == Insert.Before)
            {
                newI = initI + i;
            }
            else
            {
                newI = initI - i;
            }
            newNumber = $"[#{newI.ToString().PadLeft(4, '0')}-{secondI.ToString()}]";
            return newNumber;
        }

        public static string Number2NoteNumber(int i)
        {
            return $"[#{i.ToString().PadLeft(4, '0')}]";
        }

        public static int NoteNumber2Number(string number)
        {
            return int.Parse(number.Substring(2, 4));
        }

        public static bool IsSecondaryNumber(string number)
        {
            return number.Contains("-");
        }

        public static string NoteNum2String(int noteNum)
        {
            int octave = noteNum / 12;
            string note;
            switch (11 - noteNum % 12)
            {
                case 0:
                    note = "B";
                    break;
                case 1:
                    note = "A#";
                    break;
                case 2:
                    note = "A";
                    break;
                case 3:
                    note = "G#";
                    break;
                case 4:
                    note = "G";
                    break;
                case 5:
                    note = "F#";
                    break;
                case 6:
                    note = "F";
                    break;
                case 7:
                    note = "E";
                    break;
                case 8:
                    note = "D#";
                    break;
                case 9:
                    note = "D";
                    break;
                case 10:
                    note = "C#";
                    break;
                default:
                    note = "C";
                    break;
            }
            return $"{note}{octave}";
        }

        public static List<UNote> GetSortedNotes()
        {
            return NotesList.OrderBy(n => n.AbsoluteTime).ToList();
        }

        public static UNote GetPrevNote(UNote note)
        {
            List<UNote> notes = GetSortedNotes();
            int i = notes.IndexOf(note);
            if (i == 0) return null;
            return NotesList[i - 1];

        }

        public static UNote GetNextNote(UNote note)
        {
            List<UNote> notes = GetSortedNotes();
            int i = notes.IndexOf(note);
            if (i > notes.Count - 2) return null;
            return NotesList[i + 1];
        }

        public static long MillisecondToTick(double ms)
        {
            return MusicMath.MillisecondToTick(ms);
        }

        public static double TickToMillisecond(long ticks)
        {
            var temp = MusicMath.TickToMillisecond(ticks);
            return temp;
        }

        public static void BuildPitch()
        {

            foreach (UNote note in NotesList)
            {
                if (!note.IsRest)
                {
                    UPitch.BuildPitchData2(note);
                }
            }
            for (int i = 0; i < NotesCount-1; i++)
            {
                Recalculate();
                UNote note = NotesList[i];
                UNote noteNext = GetNextNote(note);
                UNote notePrev = GetPrevNote(note);
                if (note.IsRest) continue;

                if (note.PitchBend == null || note.IsRest) continue;


                if (noteNext.PitchBend == null || noteNext.IsRest) continue;

                // average pitch
                if (!note.IsRest)
                {
                    AveragePitch(note, noteNext);
                }

                //remove excess pitch
                if (notePrev == null || notePrev.IsRest)
                {
                    var xPre = Ust.TickToMillisecond((long)note.PitchBend.Points[0].X);
                    if (xPre < -note.pre)
                    {
                        int tokick = (int)Math.Round(-(note.pre + xPre) / Settings.IntervalMs);
                        note.PitchBend.Array = note.PitchBend.Array.Skip(tokick).ToArray();
                    }
                }
            }
        }

        static void AveragePitch(UNote note, UNote noteNext)
        {
            int[] thisPitch = note.PitchBend.Array;
            int[] nextPitch = noteNext.PitchBend.Array;
            int length = (int)(noteNext.pre / Settings.IntervalMs);
            int start = thisPitch.Length - length;
            int C = (note.NoteNum - noteNext.NoteNum) * 100;
            for (int k = 0; k < length; k++)
            {
                thisPitch[k + start] = nextPitch[k] - C;
            }
        }

        public static void Recalculate()
        {
            foreach (UNote note in NotesList)
            {
                note.Recalculate();
            }
        }
    }
}
