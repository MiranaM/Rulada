﻿using System;
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
            double.Parse((data["[#SETTING]"]["Tempo"]), new CultureInfo("ja-JP").NumberFormat);
            

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
            note.RequiredLength = note.GetRequiredLength();
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

        //public static void Split(string number)
        //{
        //    // Bisect all notes 
        //    Dictionary<string, UNote> NewNotes = new Dictionary<string, UNote> { };
        //    List<string> NewNumbers = new List<string> { };
        //    string initNumber = Ust.Numbers[0];
        //    int i = 0;
        //    string currentNumber;
        //    UNote note = uNotes[number];
        //    UNote noteN = note.Copy();
        //    int initLength = note.Length;
        //    note.Length = initLength * 2 / 3;
        //    noteN.Length = initLength - note.Length;
        //    noteN.SetDefaultNoteSettings();

        //    currentNumber = UpgradeNumber(initNumber, i);
        //    NewNumbers.Add(currentNumber);
        //    NewNotes[currentNumber] = note;

        //    currentNumber = UpgradeNumber(initNumber, i, 10);
        //    NewNumbers.Add(currentNumber);
        //    NewNotes[currentNumber] = noteN;
        //    Ust.Numbers = NewNumbers.ToArray();
        //    Ust.uNotes = NewNotes;
        //}

        //public static void Merge(List<string> numbers)
        //{
        //    // Merge all notes to 1

        //    UNote NewNote = uNotes[Numbers[0]].Copy();
        //    string NewNumber = $"[#{Numbers.First().Substring(2, 4)}-{Numbers.Last().Substring(2, 4)}]";
        //    List<string> lyrics = new List<string> { };
        //    List<int> length = new List<int> { };

        //    foreach (string number in numbers)
        //    {
        //        UNote note = uNotes[number];
        //        lyrics.Add(note.Lyric);
        //        length.Add(note.Length);
        //        uNotes[number] = new UNote();
        //    }
        //    string NewLyric = String.Join(" ", lyrics);
        //    int NewLength = length.Sum();
        //    NewNote.Length = NewLength;
        //    NewNote.Lyric = NewLyric;

        //    uNotes[NewNumber] = NewNote;
        //    Numbers = new string[] { NewNumber };

        //}

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

        public static void InsertNote(string number, string lyric, Insert insert = Insert.Before)
        {
        //    string newNumber = UpgradeNumber(number, 0, 10, insert: insert);
        //    List<string> listNumbers = Numbers.ToList();
        //    int newIndex = listNumbers.IndexOf(number);
        //    listNumbers.Insert(newIndex + 1, newNumber);
        //    Ust.Numbers = listNumbers.ToArray();

        //    UNote notePrev = uNotes[number];
        //    UNote noteN = notePrev.Copy();
        //    noteN.Lyric = lyric;
        //    int len = GetLength(lyric);
        //    if (len > notePrev.Length)
        //    {
        //        // note length is too small
        //        return;
        //    }
        //    noteN.Length = len;
        //    notePrev.Length -= noteN.Length;

        //    uNotes[newNumber] = noteN;
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
            return MusicMath.MillisecondToTick(ms, Settings.Tempo, Settings.BeatUnit, Settings.Resolution);
        }

        public static double TickToMillisecond(long tick)
        {
            var temp = MusicMath.TickToMillisecond(tick, Settings.Tempo, Settings.BeatUnit, Settings.Resolution);
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
                UNote note = NotesList[i];
                UNote noteNext = NotesList[i + 1];
                UNote notePrev = GetPrevNote(note);
                if (note.IsRest) continue;
                double pre, ovl, preNext, ovlNext;
                pre = note.Oto.Preutter;
                ovl = note.Oto.Overlap;
                if (notePrev != null && TickToMillisecond(note.Length) / 2 < pre - ovl)
                {
                    pre = note.Oto.Preutter / (note.Oto.Preutter - note.Oto.Overlap) * note.RequiredLength / 2;
                    ovl = note.Oto.Overlap / (note.Oto.Preutter - note.Oto.Overlap) * note.RequiredLength / 2;
                }
                if (noteNext.HasOto)
                {
                    preNext = noteNext.Oto.Preutter;
                    ovlNext = noteNext.Oto.Overlap;
                }
                else
                {
                    preNext = 40;
                    ovlNext = 40;
                }
                if (!noteNext.IsRest && TickToMillisecond(noteNext.Length) / 2 < preNext - ovlNext)
                {
                    preNext = noteNext.Oto.Preutter / (noteNext.Oto.Preutter - noteNext.Oto.Overlap) * noteNext.RequiredLength / 2;
                    ovlNext = noteNext.Oto.Overlap / (noteNext.Oto.Preutter - noteNext.Oto.Overlap) * noteNext.RequiredLength / 2;
                }

                if (note.PitchBend == null || note.IsRest) continue;

                // remove excess pitch
                var xPre = Ust.TickToMillisecond((long)note.PitchBend.Points[0].X);
                if (xPre < - pre)
                {
                    int tokick = (int) Math.Round(-(pre + xPre) / Settings.IntervalMs);
                    note.PitchBend.Array = note.PitchBend.Array.Skip(tokick).ToArray();
                }

                if (noteNext.PitchBend == null || noteNext.IsRest) continue;

                // end pitch from next note
                int length = (int) Math.Round(preNext / Settings.IntervalMs);
                int[] part = NotesList[i + 1].PitchBend.Array.Take(length).ToArray();
                int noteLast = NotesList[i].PitchBend.Array.Length - 1;
                int partLast = part.Length < NotesList[i].PitchBend.Array.Length ? part.Length - 1 : noteLast;
                int C = (NotesList[i].NoteNum - NotesList[i + 1].NoteNum) * 100;
                for (int k = 0; k < partLast+1; k++)
                {
                    NotesList[i].PitchBend.Array[noteLast - k] = part[partLast - k] - C;
                }
            }
        }
    }
}
