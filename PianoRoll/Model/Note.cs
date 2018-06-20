using PianoRoll.Control;
using PianoRoll.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PianoRoll.Model
{
    public struct Envelope
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

        public Envelope(Note note)
        {
            p1 = note.ovl;
            p2 = note.pre;
            p3 = 30;
            p4 = 0;
            p5 = 0;
            v1 = 60;
            v2 = 100;
            v3 = 60;
            v4 = 0;
            v5 = 100;
            Note next = note.GetNext();
            if (next != null) p3 = next.ovl;
        }
    }

    public class Note
    {
        #region variables
        private int _length;
        private string _lyric;
        private int _noteNum;
        private long _absoluteTime;
        private Envelope _envelope;
        private VibratoExpression _vibrato;
        private Phoneme _phoneme;

        public Part Part;

        public dynamic Length { get => _length; set { SetLength(value); } }
        public dynamic Lyric { get => _lyric; set { SetLyric(value); } }
        public dynamic NoteNum { get => _noteNum; set { SetNoteNum(value); } }
        public dynamic Envelope { get => GetEnvelope(); set { SetEnvelope(value); } }
        public dynamic AbsoluteTime { get => _absoluteTime; set { _absoluteTime = (long)value; } }
        public dynamic Vibrato { get => _vibrato; set { SetVibrato(value); } }

        public double RequiredLength { get { return GetRequiredLength(); } }
        public int Velocity { get; set; }
        public int Intensity { get; set; }
        public int Modulation { get; set; }
        public string Flags { get; set; }
        public PitchBendExpression PitchBend { get; set; }
        public NoteControl NoteControl { get => _noteControl; set => SetNoteControl(value); }
        public Phoneme Phoneme { get => _phoneme; set => _phoneme = value; }
        public Phoneme DefaultPhoneme { get { return Model.Phoneme.GetDefault(Lyric); } }

        public double STP { get; set; }

        public double pre { get; private set; }
        public double ovl { get; private set; }
        public double stp { get; private set; }
        public double lengthAdd { get; private set; }

        public Envelope GetEnvelope()
        {
            return hasEnvelope ? _envelope : new Envelope(this);
        }

        public bool HasPhoneme { get { return Phoneme != null; } }
        private NoteControl _noteControl;
        bool hasEnvelope = false;
        #endregion

        #region Setters
        private void SetLength(int value) { _length = value; if (value <= 0) Delete(); }
        private void SetLength(double value) { _length = (int)value; if (value <= 0) Delete(); }
        private void SetLength(float value) { _length = (int)value; if (value <= 0) Delete(); }

        private void SetNoteNum(int value) { _noteNum = value; }
        private void SetNoteNum(double value) { _noteNum = (int)value; }
        private void SetNoteNum(float value) { _noteNum = (int)value; }

        private void SetEnvelope(Envelope value) { _envelope = value; hasEnvelope = true; }
        private void SetEnvelope(string value)
        {
            string[] ops = value.Split(',');
            _envelope = new Envelope
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
            hasEnvelope = true;
        }
        private void SetEnvelope(string[] value)
        {
            _envelope = new Envelope
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
            hasEnvelope = true;
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
            _lyric = lyric;
            var temp = lyric;
            if (PartEditor.UseTrans) temp = TransitionTool.Process(this);
            if (PartEditor.UseDict) temp = Part.Track.Singer.SingerDictionary.Process(temp);
            Phoneme = Part.Track.Singer.FindPhoneme(temp);
        }

        private void SetNoteControl(NoteControl noteControl)
        {
            noteControl.note = this;
            _noteControl = noteControl;
            NewLyric(Lyric);
        }
        #endregion

        public Note GetNext() { return Part.GetNextNote(this); }
        public Note GetPrev() { return Part.GetPrevNote(this); }

        public Note(Part part)
        {
            Part = part;
            Modulation = 0;
            Intensity = 100;
            Velocity = 100;
            Length = Settings.Resolution;
            Lyric = Settings.DefaultLyric;
            PitchBend = new PitchBendExpression();
        }

        public void NewLyric(string lyric)
        {
            Lyric = lyric;
            if (HasPhoneme) NoteControl.SetText(Lyric, Phoneme.Alias);
            else NoteControl.SetText(Lyric);
        }

        public void SubmitPosLen()
        {
            if (NoteControl != null)
            {
                AbsoluteTime = MusicMath.GetAbsoluteTime(Canvas.GetLeft(NoteControl));
                NoteNum = MusicMath.GetNoteNum(Canvas.GetTop(NoteControl)) - 1;
            }
        }

        public void Trim()
        {
            Note next = GetNext();
            if (next != null && Length > next.AbsoluteTime - AbsoluteTime)
                Length = next.AbsoluteTime - AbsoluteTime;
        }

        public void Delete()
        {
            Part.Delete(this);
            NoteControl.Delete();
        }

        /// <summary>
        /// Snap AbsoluteTime and Length to project grid
        /// </summary>
        public void Snap()
        {
            AbsoluteTime = MusicMath.SnapAbsoluteTime(AbsoluteTime);
            Length = MusicMath.SnapAbsoluteTime(Length);
        }

        public void RecalculatePreOvl()
        {
            Note notePrev = Part.GetPrevNote(this);
            pre = HasPhoneme ? Phoneme.Preutter : 30;
            ovl = HasPhoneme ? Phoneme.Overlap : 30;
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
            var len = MusicMath.TickToMillisecond(Length);
            requiredLength = len + pre;
            if (next != null && next.HasPhoneme)
            {
                requiredLength -= next.Phoneme.Preutter;
                requiredLength += next.Phoneme.Overlap;
            }
            requiredLength = Math.Ceiling((requiredLength + stp + 25) / 50) * 50;
            return requiredLength;
        }

        public bool IsConnectedLeft()
        {
            // true если нет паузы после предыдущей ноты
            Note prev = GetPrev();
            if (prev == null) return false;
            else return prev.AbsoluteTime + prev.Length == AbsoluteTime;
        }

        public bool IsConnectedRight()
        {
            // true если нет паузы перед следующей нотой
            Note next = GetNext();
            if (next == null) return false;
            else return AbsoluteTime + Length == next.AbsoluteTime;
        }
    }
}
