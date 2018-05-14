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
        public double p1;
        public double p2;
        public double p3;
        public double p4;
        public double p5;
        public double v1;
        public double v2;
        public double v3;
        public double v4;
        public double v5;
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
            "Envelope"
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
        public PitchBendExpression PitchBend;
        public VibratoExpression Vibrato;
        public string UNumber;
        public long AbsoluteTime;
        public int Volume = 80;
        public UOto Oto { get; set; }
        public double RequiredLength { get; set; }
        public bool HasOto = false;

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
                case "NoteNum":
                    this[parameter] = int.Parse(value) + 12;
                    break;
                case "Length":
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
                    this.Envelope.p1 = double.Parse(ops[0]);
                    this.Envelope.p2 = double.Parse(ops[1]);
                    this.Envelope.p3 = double.Parse(ops[2]);
                    this.Envelope.v1 = double.Parse(ops[3]);
                    this.Envelope.v2 = double.Parse(ops[4]);
                    this.Envelope.v3 = double.Parse(ops[5]);
                    this.Envelope.v4 = double.Parse(ops[6]);
                    // 7 -- %
                    this.Envelope.p4 = double.Parse(ops[8]);
                    this.Envelope.p5 = ops.Length > 9 ? double.Parse(ops[9]) : 0;
                    this.Envelope.v5 = ops.Length > 9 ? double.Parse(ops[10]) : 100;
                    break;
                case "VBR":
                    break;
                case "PBS":
                case "PBW":
                case "PBY":
                case "PBM":
                    break;
                case "Flags":
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
                    this[parameter] = new UEnvelope { };
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
            Intensity = Ust.uDefaultNote.Intensity;
            Modulation = Ust.uDefaultNote.Modulation;
            Envelope = Ust.uDefaultNote.Envelope;
            PitchBend = Ust.uDefaultNote.PitchBend;
        }

        public void ResetAlias()
        {
            foreach (string parameter in AliasParameters.Keys)
            {
                GotParameters.Remove(parameter);
            }
            AliasParameters = new Dictionary<string, dynamic> { };
        }
    }
}
