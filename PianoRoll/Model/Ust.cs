using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PianoRoll.Util;

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
        static public UNote uPrev;
        static public UNote uNext;
        static public List<UNote> NotesList = new List<UNote>();
        static public bool hasPrev = false;
        static public bool hasNext = false;
        static public string uVersion;
        static public int NotesCount = 0;

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

        public static void Save()
        {
            Text = ToStrings();
            File.WriteAllText(Dir, Text);
            Console.WriteLine("Successfully saved UST.");
        }

        public static void Save(string dir)
        {
            Text = ToStrings();
            File.WriteAllText(dir, Text);
            Console.WriteLine("Successfully saved debug UST.");
        }

        private static void Read(dynamic data, string[] sections)
        {
            string stage = "Init";
            stage = "Version";
            // Reading string
            // uVersion = data["[#VERSION]"];
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
            Settings.Tempo = double.Parse((data["[#SETTING]"]["Tempo"]).Replace(".", ",")); //замена точки на запятую
            

            // Sections - Version, Settings;
            stage = "Notes count";
            NotesCount = data["SectionsNumber"] - 3;
            if (data.ContainsKey("[#PREV]")) NotesCount--;
            if (data.ContainsKey("[#NEXT]")) NotesCount--;

            long absoluteTime = 0;
            // Reading prev
            stage = "Prev";
            hasPrev = data.ContainsKey("[#PREV]");
            if (hasPrev) uPrev = NoteRead(data, "PREV", ref absoluteTime, out string number);

            // Reading notes
            stage = "Notes";
            //uNotes = new Dictionary<string, UNote> { };
            //Numbers = new string[NotesCount];
            stage = "Searching first note number";
            int firstNote = -1;
            if (hasPrev) firstNote = NoteNumber2Number(sections[4]);
            else firstNote = NoteNumber2Number(sections[3]);

            for (int i = 0; i < NotesCount; i++)
            {
                stage = $"Note: {i}";
                UNote note = NoteRead(data, (i + firstNote).ToString(), ref absoluteTime, out string number);
                //uNotes[number] = note;
                //Numbers[i] = number;
                NotesList.Add(note);
            }

            // Reading next
            stage = "Next";
            hasNext = data.ContainsKey("[#NEXT]");
            if (hasNext) uNext = NoteRead(data, "NEXT", ref absoluteTime, out string number);
            Console.WriteLine("Read UST successfully");
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
            uDefaultNote.Set("Envelope", "0,21,35,0,100,100,0,%,0");
        }

        private static UNote NoteRead(dynamic data, string which, ref long absoluteTime, out string number)
        {
            // May be #PREV, #0000 .... #NNNN, #NEXT
            if (int.TryParse(which, out int tempInt))
            {
                number = Number2NoteNumber(tempInt);
            }
            else
            {
                number = $"[#{which}]";
            }

            UNote note = new UNote();
            note.Vibrato = new VibratoExpression(note);
            note.SetDefaultNoteSettings();
            int i = 0;
            Console.WriteLine($"Setting values for note {number}");
            foreach (string parameter in data[number].Keys)
            {
                Console.WriteLine($"\tTrying set parameter  {parameter}");
                var value = data[number][parameter];
                note.Set(parameter, value);
                note.isRest = note.Lyric == "";
                i++;
            }
            note.UNumber = number;
            note.AbsoluteTime = absoluteTime;
            note.RequiredLength = Math.Ceiling((double) note.Length / 50 + 1) * 50;
            absoluteTime += (long)note.Length;
            if (data.ContainsKey("VBR")) note.Vibrato = UPitch.VibratoFromUst(data[number]["VBR"], note);
            UPitch.PitchFromUst(data[number], ref note);
            return note;
        }

        public static string ToStrings()
        {
            string text = "";
            List<string> vartext = new List<string> { };
            vartext.Add("[#VERSION]");
            vartext.Add(uVersion);
            vartext.Add("[#SETTING]");
            foreach (string setting in USettingsList)
            {
                vartext.Add($"{setting}={uSettings[setting]}");
            }
            if (hasPrev)
            {
                vartext.Add("[#PREV]");
                foreach (string line in uPrev.ToStrings())
                {
                    vartext.Add(line);
                }
            }
            foreach (UNote note in NotesList)
            {
                vartext.Add(note.UNumber);
                foreach (string line in note.ToStrings())
                {
                    vartext.Add(line);
                }
            }
            if (hasNext)
            {
                vartext.Add("[#NEXT]");
                foreach (string line in uNext.ToStrings())
                {
                    vartext.Add(line);
                }
            }
            foreach (string line in vartext)
            {
                text += line + "\r\n";
            }
            return text;
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

        public static void SetLyric(string[] lyric)
        {
            int i = 0;
            foreach (UNote note in NotesList)
            {
                note.SetLyric(lyric[i]);
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

        public static void SetLyricPrev(string lyric)
        {
            if (hasPrev) uPrev.SetLyric(lyric);
        }

        public static void SetLyricNext(string lyric)
        {
            if (hasNext) uNext.SetLyric(lyric);
        }

        public static void SetLyric(string[] lyric, string lyricPrev, string lyricNext)
        {
            SetLyric(lyric);
            SetLyricPrev(lyricPrev);
            SetLyricNext(lyricNext);
        }

        public static void SetLyric(string[] lyric, string otherLyric = "")
        {
            SetLyric(lyric);
            if (hasNext && hasPrev) SetLyricNext(otherLyric);
            else if (hasPrev) SetLyricPrev(otherLyric);
            else if (hasNext) SetLyricNext(otherLyric);
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
            return MusicMath.TickToMillisecond(tick, Settings.Tempo, Settings.BeatUnit, Settings.Resolution);
        }
    }
}
