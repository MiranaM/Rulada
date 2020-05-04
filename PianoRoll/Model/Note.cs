using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using PianoRoll.Control;
using PianoRoll.Util;

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
            p1 = note.Ovl;
            p2 = note.Pre;
            p3 = 30;
            p4 = 0;
            p5 = 0;
            v1 = 60;
            v2 = 100;
            v3 = 60;
            v4 = 0;
            v5 = 100;
            var next = note.GetNext();
            if (next != null) p3 = next.Ovl;
        }
    }

    public class Note
    {
        #region variables

        private int length;
        private string lyric;
        private int noteNum;
        private long absoluteTime;
        private Envelope envelope;
        private VibratoExpression vibrato;
        private string phonemes;

        public bool IsRender;

        public Part Part;

        public dynamic Length
        {
            get => length;
            set => SetLength(value);
        }

        public dynamic Lyric
        {
            get => lyric;
            set => SetLyric(value);
        }

        public dynamic NoteNum
        {
            get => noteNum;
            set => SetNoteNum(value);
        }

        public dynamic Envelope
        {
            get => GetEnvelope();
            set => SetEnvelope(value);
        }

        public dynamic AbsoluteTime
        {
            get => absoluteTime;
            set => absoluteTime = (long) value;
        }

        public dynamic Vibrato
        {
            get => vibrato;
            set => SetVibrato(value);
        }

        public double RequiredLength => GetRequiredLength();

        public int Velocity { get; set; }
        public int Intensity { get; set; }
        public int Modulation { get; set; }
        public string Flags { get; set; }
        public PitchBendExpression PitchBend { get; set; }

        public NoteControl NoteControl
        {
            get => noteControl;
            set => SetNoteControl(value);
        }

        public Phoneme Phoneme { get; set; }

        public Phoneme DefaultPhoneme => Model.Phoneme.GetDefault(Lyric);

        public string Phonemes { get; set; }

        public double STP { get; set; }

        public double Pre { get; private set; }
        public double Ovl { get; private set; }
        public double Stp { get; private set; }
        public double LengthAdd { get; private set; }

        public Envelope GetEnvelope()
        {
            return hasEnvelope ? envelope : new Envelope(this);
        }

        public bool HasPhoneme => Phonemes != null;
        private NoteControl noteControl;
        private bool hasEnvelope;

        #endregion

        public Note GetNext()
        {
            return Part.GetNextNote(this);
        }

        public Note GetPrev()
        {
            return Part.GetPrevNote(this);
        }

        public Note(Part part, double lengthAdd, string phonemes)
        {
            Part = part;
            LengthAdd = lengthAdd;
            this.phonemes = phonemes;
            Modulation = 0;
            Intensity = 100;
            Velocity = 100;
            Length = Settings.Resolution;
            Lyric = Settings.DefaultLyric;
            PitchBend = new PitchBendExpression();
        }

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
            if (HasPhoneme)
                NoteControl.SetText(Lyric, Phonemes);
            else
                NoteControl.SetText(Lyric);
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
            var next = GetNext();
            if (next != null && Length > next.AbsoluteTime - AbsoluteTime) Length = next.AbsoluteTime - AbsoluteTime;
        }

        public void Delete()
        {
            Part.Delete(this);
            NoteControl.Delete();
        }

        /// <summary>
        ///     Snap AbsoluteTime and Length to project grid
        /// </summary>
        public void Snap()
        {
            AbsoluteTime = MusicMath.SnapAbsoluteTime(AbsoluteTime);
            Length = MusicMath.SnapAbsoluteTime(Length);
        }

        public void RecalculatePreOvl()
        {
            var notePrev = Part.GetPrevNote(this);
            Pre = 50; //HasPhoneme ? Phoneme.Preutter : 30;
            Ovl = 30; //HasPhoneme ? Phoneme.Overlap : 30;
            Stp = 0;
            double length = MusicMath.TickToMillisecond(Length);
            if (notePrev != null && MusicMath.TickToMillisecond(Length) / 2 < Pre - Ovl)
            {
                Pre = Phoneme.Preutter / (Phoneme.Preutter - Phoneme.Overlap) * (length / 2);
                Ovl = Phoneme.Overlap / (Phoneme.Preutter - Phoneme.Overlap) * (length / 2);
                Stp = Phoneme.Preutter - Pre;
                if (Pre > Phoneme.Preutter || Ovl > Phoneme.Overlap)
                    throw new Exception("Да еб вашу мать");
            }
        }

        public double GetRequiredLength()
        {
            var next = Part.GetNextNote(this);
            var prev = Part.GetPrevNote(this);
            var len = MusicMath.TickToMillisecond(Length);
            double requiredLength = len + Pre;
            if (next != null && next.HasPhoneme)
            {
                requiredLength -= next.Phoneme.Preutter;
                requiredLength += next.Phoneme.Overlap;
            }

            requiredLength = Math.Ceiling((requiredLength + Stp + 25) / 50) * 50;
            return requiredLength;
        }

        public bool IsConnectedLeft()
        {
            // true если нет паузы после предыдущей ноты
            var prev = GetPrev();
            if (prev == null)
                return false;
            return prev.AbsoluteTime + prev.Length == AbsoluteTime;
        }

        public bool IsConnectedRight()
        {
            // true если нет паузы перед следующей нотой
            var next = GetNext();
            if (next == null)
                return false;
            return AbsoluteTime + Length == next.AbsoluteTime;
        }

        #region private

        private void SetLength(int value)
        {
            length = value;
            if (value <= 0) Delete();
        }

        private void SetLength(double value)
        {
            length = (int)value;
            if (value <= 0) Delete();
        }

        private void SetLength(float value)
        {
            length = (int)value;
            if (value <= 0) Delete();
        }

        private void SetNoteNum(int value)
        {
            noteNum = value;
        }

        private void SetNoteNum(double value)
        {
            noteNum = (int)value;
        }

        private void SetNoteNum(float value)
        {
            noteNum = (int)value;
        }

        private void SetEnvelope(Envelope value)
        {
            envelope = value;
            hasEnvelope = true;
        }

        private void SetEnvelope(string value)
        {
            var ops = value.Split(',');
            envelope = new Envelope
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
            envelope = new Envelope
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

        private void SetVibrato(VibratoExpression value)
        {
            vibrato = value;
        }

        private void SetVibrato(string vbr)
        {
            var value = vbr.Split(',');
            var vibrato = new VibratoExpression();
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

            this.vibrato = vibrato;
        }

        private void SetLyric(string lyric)
        {
            this.lyric = lyric;
            var temp = lyric;
            if (PartEditor.UseDict)
                Phonemes = Part.Track.Singer.SingerDictionary.Process(lyric);
            if (PartEditor.UseTrans)
                temp = TransitionTool.Process(this);
            Phoneme = Part.Track.Singer.FindPhoneme(temp);
        }

        private void SetNoteControl(NoteControl noteControl)
        {
            noteControl.note = this;
            this.noteControl = noteControl;
            NewLyric(Lyric);
        }

        #endregion

    }
}