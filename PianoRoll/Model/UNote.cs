using PianoRoll.Control;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private int _length;
        private string _lyric;
        private int _noteNum;
        public UEnvelope _envelope;

        public dynamic Length { get => _length; set { SetLength(value); } }
        public dynamic Lyric { get => _lyric; set { SetLyric(value); } }
        public dynamic NoteNum { get => _noteNum; set { SetNoteNum(value); } }
        public dynamic Envelope { get => _envelope; set { SetEnvelope(value); } }

        public int Velocity;
        public int Intensity;
        public int Modulation;
        public string Flags;
        public PitchBendExpression PitchBend;
        public VibratoExpression Vibrato;
        public string UNumber;
        public long AbsoluteTime;
        public bool isRest = false;
        public int Volume = 80;
        public UOto Oto { get; set; }
        public double RequiredLength { get; set; }
        public bool HasOto = false;
        public NoteControl noteControl;

        private List<string> GotParameters = new List<string> { };

        //public void Set(string parameter, dynamic value)
        //{
        //    if (value is "") return;
        //    if (value is IEnumerable<string>)
        //    {
        //        value = String.Join(",", value);
        //    }

        //    Console.WriteLine($"\t{parameter}={value}");
        //    switch (parameter)
        //    {
        //        case "NoteNum":
        //            this[parameter] = int.Parse(value) - 12;
        //            break;
        //        case "Length":
        //        case "Velocity":
        //        case "Intensity":
        //        case "Modulation":
        //            this[parameter] = int.Parse(value);
        //            break;
        //        case "Envelope":
        //            string[] ops = value.Split(',');
        //            this.Envelope.p1 = double.Parse(ops[0]);
        //            this.Envelope.p2 = double.Parse(ops[1]);
        //            this.Envelope.p3 = double.Parse(ops[2]);
        //            this.Envelope.v1 = double.Parse(ops[3]);
        //            this.Envelope.v2 = double.Parse(ops[4]);
        //            this.Envelope.v3 = double.Parse(ops[5]);
        //            this.Envelope.v4 = double.Parse(ops[6]);
        //            // 7 -- %
        //            this.Envelope.p4 = double.Parse(ops[8]);
        //            this.Envelope.p5 = ops.Length > 9 ? double.Parse(ops[9]) : 0;
        //            this.Envelope.v5 = ops.Length > 9 ? double.Parse(ops[10]) : 100;
        //            break;
        //        case "VBR":
        //            break;
        //        case "PBS":
        //        case "PBW":
        //        case "PBY":
        //        case "PBM":
        //            break;
        //        case "Flags":
        //        case "Lyric":
        //            this[parameter] = value;
        //            break;
        //        default:
        //            if (parameter[0] == '@') AliasParameters[parameter] = value;
        //            else OtherParameters[parameter] = value;
        //            break;
        //    }
        //    GotParameters.Add(parameter);
        //}

        private void SetLength(string value) { _length = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); }
        private void SetLength(int value) { _length = value; }
        private void SetLength(double value) { _length = (int) value; }
        private void SetLength(float value) { _length = (int) value; }

        private void SetNoteNum(string value) { _noteNum = int.Parse(value, new CultureInfo("ja-JP").NumberFormat); }
        private void SetNoteNum(int value) { _noteNum = value; }
        private void SetNoteNum(double value) { _noteNum = (int) value; }
        private void SetNoteNum(float value) { _noteNum = (int) value; }

        private void SetEnvelope(UEnvelope value) { _envelope = value; }
        private void SetEnvelope(string value)
        {
            string[] ops = value.Split(',');
            _envelope = new UEnvelope
            {
                p1 = double.Parse(ops[0]),
                p2 = double.Parse(ops[1]),
                p3 = double.Parse(ops[2]),
                v1 = double.Parse(ops[3]),
                v2 = double.Parse(ops[4]),
                v3 = double.Parse(ops[5]),
                v4 = double.Parse(ops[6]),
                // 7 -- %
                p4 = double.Parse(ops[8]),
                p5 = ops.Length > 9 ? double.Parse(ops[9]) : 0,
                v5 = ops.Length > 9 ? double.Parse(ops[10]) : 100
            };
        }
        private void SetEnvelope(string[] value)
        {
            _envelope = new UEnvelope
            {
                p1 = double.Parse(value[0], new CultureInfo("ja-JP").NumberFormat),
                p2 = double.Parse(value[1], new CultureInfo("ja-JP").NumberFormat),
                p3 = double.Parse(value[2], new CultureInfo("ja-JP").NumberFormat),
                v1 = double.Parse(value[3], new CultureInfo("ja-JP").NumberFormat),
                v2 = double.Parse(value[4], new CultureInfo("ja-JP").NumberFormat),
                v3 = double.Parse(value[5], new CultureInfo("ja-JP").NumberFormat),
                v4 = double.Parse(value[6], new CultureInfo("ja-JP").NumberFormat),
                // 7 -- %
                p4 = double.Parse(value[8]),
                p5 = value.Length > 9 ? double.Parse(value[9], new CultureInfo("ja-JP").NumberFormat) : 0,
                v5 = value.Length > 9 ? double.Parse(value[10], new CultureInfo("ja-JP").NumberFormat) : 100
            };
        }

        private void SetLyric(string lyric)
        {
            if (lyric == "R") lyric = "";
            _lyric = lyric;
            Oto = USinger.FindOto(lyric);
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
                        //if (this[parameter].ToString() == "r") value = "rr";
                        //else value = this[parameter].ToString();
                        break;
                    default:
                        //value = this[parameter].ToString();
                        break;
                }
                //text[i] = $"{parameter}={value}";
            }
            return text;
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
                    //NewNote[parameter] = this[parameter];
                    //NewParameters.Add(parameter);
                }
            }
            NewNote.GotParameters = NewParameters;
            return NewNote;
        }

        public UNote CopyWhole()
        {
            // Copy all parameters
            UNote NewNote = new UNote();
            foreach(string parameter in GotParameters)
            {
                //NewNote[parameter] = this[parameter];
            }
            NewNote.GotParameters = GotParameters;
            return NewNote;
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

        public double GetRequiredLength()
        {
            double rl = Ust.TickToMillisecond(Length) + Oto.Preutter;
            UNote next = Ust.GetNextNote(this);
            if (next != null && !next.isRest)
            {
                rl -= next.Oto.Preutter;
                rl += next.Oto.Overlap;
            }
            return rl;
        }
    }
}
