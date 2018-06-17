using PianoRoll.Control;
using PianoRoll.Util;
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

    public class Note
    {
        private int _length;
        private string _lyric;
        private int _noteNum;
        private long _absoluteTime;
        private UEnvelope _envelope;
        private VibratoExpression _vibrato;
        private Phoneme _oto;

        public Part Part;

        public dynamic Length { get => _length; set { SetLength(value); } }
        public dynamic Lyric { get => _lyric; set { SetLyric(value); } }
        public dynamic NoteNum { get => _noteNum; set { SetNoteNum(value); } }
        public dynamic Envelope { get => _envelope; set { SetEnvelope(value); } }
        public dynamic AbsoluteTime { get => _absoluteTime; set { _absoluteTime = (long) value; } }
        public dynamic Vibrato { get => _vibrato; set { SetVibrato(value); } }

        public double RequiredLength { get { return GetRequiredLength(); } }
        public int Velocity { get; set; }
        public int Intensity { get; set; }
        public int Modulation { get; set; }
        public string Flags { get; set; }
        public PitchBendExpression PitchBend { get; set; }
        public NoteThumb NoteControl { get; set; }
        public Phoneme Phoneme { get { if (HasPhoneme) return _oto; else return Model.Phoneme.GetDefault(Lyric); } set { _oto = value; } }

        public double STP { get; set; }

        public double pre { get; private set; }
        public double ovl { get; private set; }
        public double stp { get; private set; }
        public double lengthAdd { get; private set; }

        public bool IsRest = false;
        public bool HasPhoneme = false;

        private void SetLength(int value) { _length = value; }
        private void SetLength(double value) { _length = (int) value; }
        private void SetLength(float value) { _length = (int) value; }

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
                p4 = ops.Length > 7 ? double.Parse(ops[8]) : 0,
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
                p4 = value.Length > 7 ? double.Parse(value[8]) : 0,
                p5 = value.Length > 9 ? double.Parse(value[9], new CultureInfo("ja-JP").NumberFormat) : 0,
                v5 = value.Length > 9 ? double.Parse(value[10], new CultureInfo("ja-JP").NumberFormat) : 100
            };
        }

        private void SetVibrato(VibratoExpression value) { _vibrato = value; }
        private void SetVibrato(string vbr)
        {
            string[] value = vbr.Split(',');
            VibratoExpression vibrato = new VibratoExpression();
            if (value.Count() >= 7)
            {
                vibrato.Length = double.Parse(value[0], new CultureInfo("ja-JP"));
                vibrato.Period = double.Parse(value[1], new CultureInfo("ja-JP"));
                vibrato.Depth = double.Parse(value[2], new CultureInfo("ja-JP"));
                vibrato.In = double.Parse(value[3], new CultureInfo("ja-JP"));
                vibrato.Out = double.Parse(value[4], new CultureInfo("ja-JP"));
                vibrato.Shift = double.Parse(value[5], new CultureInfo("ja-JP"));
                vibrato.Drift = double.Parse(value[6], new CultureInfo("ja-JP"));
            }
            _vibrato = vibrato;
        }

        private void SetLyric(string lyric)
        {
            if (lyric == "R") lyric = "";
            if (lyric == "") IsRest = true;
            _lyric = lyric;
            if (Part != null) Phoneme = Part.Singer.FindPhoneme(lyric);
        }

        public Note()
        {
            Modulation = 0;
            Intensity = 100;
            Velocity = 100;
        }
        
        public void Recalculate()
        {
            Note notePrev = Part.GetPrevNote(this);
            pre = IsRest ? 30 : Phoneme.Preutter;
            ovl = IsRest ? 30 : Phoneme.Overlap;
            stp = 0;
            double length = MusicMath.TickToMillisecond(Length);
            if (notePrev != null && MusicMath.TickToMillisecond(Length) / 2 < pre - ovl)
            {
                pre = Phoneme.Preutter / (Phoneme.Preutter - Phoneme.Overlap) * (length / 2);
                ovl = Phoneme.Overlap / (Phoneme.Preutter - Phoneme.Overlap) * (length / 2);
                stp = Phoneme.Preutter - pre;
                if (pre > Phoneme.Preutter || ovl > Phoneme.Overlap)
                    throw new Exception("Да еб вашу мать");
            }
        }

        public double GetRequiredLength()
        {
            double requiredLength;
            Note next = Part.GetNextNote(this);
            Note prev = Part.GetPrevNote(this);

            //var ConsonantStretch = Math.Pow(2, 1 - Velocity / 100 );
            //var ConsonantModified = Phoneme.Consonant * ConsonantStretch;
            //var stpModified = stp * ConsonantStretch;
            //var PreutterModified = pre * ConsonantStretch;
            //var OverlapModified = ovl * ConsonantStretch;
            //var SpokenLength = Ust.TickToMillisecond(Length) + PreutterModified - next.pre + next.ovl;
            //requiredLength = Math.Ceiling((SpokenLength + stpModified + 25) / 50) * 50;

            var len = MusicMath.TickToMillisecond(Length);
            requiredLength = len + pre;
            if (next != null && !next.IsRest)
            {
                requiredLength -= next.Phoneme.Preutter;
                requiredLength += next.Phoneme.Overlap;
            }
            requiredLength = Math.Ceiling((requiredLength + stp + 25) / 50 ) * 50;

            return requiredLength;
        }
    }
}
