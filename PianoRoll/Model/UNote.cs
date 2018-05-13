using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model
{
    public struct UEnvelope
    {
        public int p1;
        public int p2;
        public int p3;
        public int p4;
        public int p5;
        public int v1;
        public int v2;
        public int v3;
        public int v4;
        public int v5;
    }

    public class UNote
    {
        static public string[] Parameters =
        {
            "Length",
            "Lyric",
            "NoteNum",
            "Velocity",
            "Intensity",
            "Modulation",
            "Flags",
            "Envelope",
            "PBW",
            "PBS"
        };
        private Dictionary<string, dynamic> OtherParameters = new Dictionary<string, dynamic> { };
        private Dictionary<string, dynamic> AliasParameters = new Dictionary<string, dynamic> { };
        public int Length;
        public string Lyric;
        public int NoteNum;
        public int Velocity;
        public int Intensity;
        public int Modulation;
        public string Flags;
        public UEnvelope Envelope;
        public string PBW;
        public string PBS;
        public string UNumber;
        public long AbsoluteTime;
        public int Volume = 80;
        public UOto Oto { get; set; }
        public int RequiredLength;

        private List<string> GotParameters = new List<string> { };

        public void Set(string parameter, dynamic value)
        {
            if (value is "") return;
            if (value is IEnumerable<string>)
            {
                value = String.Join(",", value);
            }

            Console.WriteLine($"\t{parameter}={value}");
            switch (parameter)
            {
                case "Length":
                case "NoteNum":
                case "Velocity":
                case "Intensity":
                case "Modulation":
                    this[parameter] = int.Parse(value);
                    break;
                case "Lyric":
                    if (value == "rr")
                    {
                        this[parameter] = "r";
                    }
                    else if (value == "R")
                    {
                        this[parameter] = "";
                    }
                    else this[parameter] = value;
                    break;
                case "Envelope":
                    string[] ops = value.Split(',');
                    this.Envelope.p1 = int.Parse(ops[0]);
                    this.Envelope.p2 = int.Parse(ops[1]);
                    this.Envelope.p3 = int.Parse(ops[2]);
                    this.Envelope.v1 = int.Parse(ops[3]);
                    this.Envelope.v2 = int.Parse(ops[4]);
                    this.Envelope.v3 = int.Parse(ops[5]);
                    this.Envelope.v4 = int.Parse(ops[6]);
                    // 7 -- %
                    this.Envelope.p4 = int.Parse(ops[8]);
                    this.Envelope.p5 = ops.Length > 9 ? int.Parse(ops[9]) : 0;
                    this.Envelope.v5 = ops.Length > 9 ? int.Parse(ops[10]) : 100;
                    break;
                case "Flags":
                case "PBW":
                case "PBS":
                    this[parameter] = value;
                    break;
                default:
                    if (parameter[0] == '@') AliasParameters[parameter] = value;
                    else OtherParameters[parameter] = value;
                    break;
            }
            GotParameters.Add(parameter);
        }

        public string[] ToStrings()
        {
            if (GotParameters.Count == 0)
            {
                return new string[] { "" };
            }
            string[] text = new string[GotParameters.Count];
            for (int i = 0; i < GotParameters.Count; i++)
            {
                string parameter = GotParameters[i];
                string value;
                switch (parameter)
                {
                    case "Lyric":
                        if (this[parameter].ToString() == "r") value = "rr";
                        else value = this[parameter].ToString();
                        break;
                    default:
                        value = this[parameter].ToString();
                        break;
                }
                text[i] = $"{parameter}={value}";
            }
            return text;
        }

        // Для обращения через имя параметра
        public object this[string fieldName]
        {
            get
            {
                if (GotParameters.Contains(fieldName))
                {
                    if (UNote.Parameters.Contains(fieldName))
                    {
                        var field = this.GetType().GetField(fieldName);
                        return field.GetValue(this);
                    }
                    if (OtherParameters.ContainsKey(fieldName))
                    {
                        return OtherParameters[fieldName];
                    }
                    if (AliasParameters.ContainsKey(fieldName))
                    {
                        return AliasParameters[fieldName];
                    }
                }
                throw new KeyNotFoundException($"Parameter {fieldName} is not set");
            }
            set
            {
                if (UNote.Parameters.Contains(fieldName))
                {
                    var field = this.GetType().GetField(fieldName);
                    field.SetValue(this, value);
                    return;
                }
                if (fieldName[0] == '@')
                {
                    AliasParameters[fieldName] = value;
                }
                else
                {
                    OtherParameters[fieldName] = value;
                }
                if (!GotParameters.Contains(fieldName))
                {
                    GotParameters.Add(fieldName);
                }
                return;
            }
        }

        public bool IsSet(string parameter)
        {
            return GotParameters.Contains(parameter);
        }

        public UNote Copy()
        {
            // Copy only main parameters
            UNote NewNote = new UNote();
            List<string> NewParameters = new List<string> { };
            foreach(string parameter in GotParameters)
            {
                if (Parameters.Contains(parameter))
                {
                    NewNote[parameter] = this[parameter];
                    NewParameters.Add(parameter);
                }
                //else
                //{
                //    NewNote.SetDefault(parameter);
                //    NewParameters.Add(parameter);
                //}
            }
            NewNote.GotParameters = NewParameters;
            return NewNote;
        }

        public void SetDefault(string parameter)
        {
            switch (parameter)
            {
                case "Flags":
                    this[parameter] = "";
                    break;
                case "Envelope":
                    this[parameter] = new string[] { };
                    break;
                default:
                    Set(parameter, "");
                    break;
            }

        }

        public UNote CopyWhole()
        {
            // Copy all parameters
            UNote NewNote = new UNote();
            foreach(string parameter in GotParameters)
            {
                NewNote[parameter] = this[parameter];
            }
            NewNote.GotParameters = GotParameters;
            return NewNote;
        }

        public void SetLyric(string lyric)
        {
            Lyric = lyric;
        }

        public void SetDefaultNoteSettings()
        {
            // We will apply this to "r" note which we won't consider Rest
            this["Intensity"] = Ust.uDefaultNote.Intensity;
            this["Modulation"] = Ust.uDefaultNote.Modulation;
            this["Envelope"] = Ust.uDefaultNote.Envelope;
            this["PBS"] = Ust.uDefaultNote.PBS;
            this["PBW"] = Ust.uDefaultNote.PBW;
        }

        public void ResetAlias()
        {
            foreach (string parameter in AliasParameters.Keys)
            {
                GotParameters.Remove(parameter);
            }
            AliasParameters = new Dictionary<string, dynamic> { };
        }

        public void SendToResampler(string resampler, string cache, double Tempo, object PitchData)
        {
            string ops = string.Format
            (
                "{0} {1:D} {2} {3} {4:D} {5} {6} {7:D} {8:D} {9} {10}",
                NoteNum,
                Velocity,
                Flags,
                Oto.Offset,
                RequiredLength,
                Oto.Consonant,
                Oto.Cutoff,
                Volume,
                0,
                Tempo,
                Pitch.BuildPitchData(this)
            );
            string request = $"\"{resampler}\" \"{Oto.File}\" \"{cache}\" {ops}";
            if (!Directory.Exists("temp")) Directory.CreateDirectory("temp");
            var dir = Path.Combine("temp", "resampler.bat");
            File.WriteAllText(dir, request);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = @"temp/resampler.bat";
            proc.StartInfo.WorkingDirectory = @"temp";
            proc.Start();
        }

        public void SendToWavtool(string tool, string output, string cache)
        {
            string ops = string.Format
            (
                "{0} {1}@{2}{3} {4} {5}",
                this["STP"],
                Length,
                Oto.Preutter < 0 ? "-" : "+",
                Math.Abs(Oto.Preutter),
                Envelope.p1,
                Envelope.p2
            );
            string opsNote;
            if (Lyric == "") opsNote = "";
            else
            {
                opsNote = string.Format
                (
                    "{0} {1} {2} {3} {4} {5} {6} {7}",
                    Envelope.p3,
                    Envelope.v1,
                    Envelope.v2,
                    Envelope.v3,
                    Oto.Overlap,
                    Envelope.p4,
                    Envelope.p5,
                    Envelope.v5
                );
            }
            string request = $"\"{tool}\" \"{output}\" \"{cache}\" {ops} {opsNote}";
            if (!Directory.Exists("temp")) Directory.CreateDirectory("temp");
            var dir = Path.Combine("temp", "tool.bat");
            File.WriteAllText(dir, request);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = @"temp/tool.bat";
            proc.StartInfo.WorkingDirectory = @"temp";
            proc.Start();

        }
    }
}
