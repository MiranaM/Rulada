using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using PianoRoll.Control;
using PianoRoll.Model.Pitch;
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

        public Envelope(Note note, Note next = null)
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
            if (next != null)
                p3 = next.Ovl;
        }

        public void Check(Note note, Note next)
        {
            var sustain = MusicMath.Current.TickToMillisecond(note.FinalLength) + note.StraightPre - next?.StraightPre;
            if (sustain <= 0)
                throw new Exception();
        }
    }

    public class Note
    {
        #region variables

        public virtual bool IsRender => false;

        public Part Part;

        public int Length { get; set; }
        public long AbsoluteTime { get; set; }
        public int NoteNum { get; set; }
        public string Lyric { get; set; }
        public Envelope Envelope { get; private set; }
        public VibratoExpression Vibrato { get; set; }

        public int Velocity { get; set; }
        public int Intensity { get; set; }
        public int Modulation { get; set; }
        public string Flags { get; set; }
        public PitchBendExpression PitchBend { get; set; }

        public NoteControl NoteControl { get; private set; }

        public Oto Oto { get; set; }
        public Oto SafeOto => Oto != null ? Oto : DefaultOto;

        public Oto DefaultOto => GetDefaultOto(Lyric);

        public string Phonemes { get; set; }

        public double Pre { get; set; }
        public double Ovl { get; set; }
        public double Stp { get; set; }
        public double StraightPre => Pre - Ovl;

        public bool HasOto => Oto != null;

        #endregion

        public Note GetNext()
        {
            return Part.GetNextNote(this);
        }

        public Note GetPrev()
        {
            return Part.GetPrevNote(this);
        }

        public override string ToString()
        {
            return $"{Lyric} [{Phonemes}] {{{SafeOto.Alias}}}";
        }

        public Note(Part part)
        {
            Part = part;
            Modulation = 0;
            Intensity = 100;
            Velocity = 100;
            Length = Settings.Current.Resolution;
            Lyric = Settings.Current.DefaultLyric;
            PitchBend = new PitchBendExpression();
        }

        public void SetNewLyric(string lyric)
        {
            ProcessLyric(lyric);
            if (Phonemes != string.Empty)
                NoteControl.SetText(Lyric, Phonemes);
            else
                NoteControl.SetText(Lyric);
        }

        public void ProcessLyric(string lyric)
        {
            Lyric = lyric;
            var temp = lyric;
            if (PartEditor.UseDict)
                Phonemes = Part.Track.Singer.SingerDictionary.Process(lyric);
            if (PartEditor.UseTrans && this is RenderNote renderNote)
                temp = TransitionTool.Current.Process(renderNote, null,false);
            Oto = Part.Track.Singer.FindOto(temp != string.Empty ? temp : Phonemes);
        }

        public virtual int FinalLength => Length;
        public virtual long FinalPosition => AbsoluteTime;

        public void SubmitPosLen()
        {
            if (NoteControl != null)
            {
                AbsoluteTime = MusicMath.Current.GetAbsoluteTime(Canvas.GetLeft(NoteControl));
                NoteNum = MusicMath.Current.GetNoteNum(Canvas.GetTop(NoteControl)) - 1;
            }
        }

        public void Delete()
        {
            Part.Delete(this);
            if (NoteControl != null)
                NoteControl.Delete();
        }

        /// <summary>
        ///     Snap AbsoluteTime and Length to project grid
        /// </summary>
        public void Snap()
        {
            AbsoluteTime = (long)MusicMath.Current.SnapAbsoluteTime(AbsoluteTime);
            Length = (int)MusicMath.Current.SnapAbsoluteTime(Length);
        }

        public void RecalculatePreOvl(Note notePrev)
        {
            var oto = SafeOto;
            Pre = oto.Preutter;
            Ovl = oto.Overlap;
            Stp = 0;
        }

        public bool IsConnectedLeft()
        {
            // true если нет паузы после предыдущей ноты
            return IsConnectedLeft(GetPrev());
        }

        public bool IsConnectedLeft(Note prev)
        {
            if (prev == null)
                return false;
            return prev.FinalPosition + prev.FinalLength == FinalPosition;
        }

        public bool IsConnectedRight()
        {
            // true если нет паузы перед следующей нотой
            var next = GetNext();
            if (next == null)
                return false;
            return FinalPosition + FinalLength == next.FinalPosition;
        }

        public void CreateEnvelope(Note next = null)
        {
            Envelope = new Envelope(this, next);
        }

        public void SetEnvelope(string value)
        {
            var ops = value.Split(',');
            Envelope = new Envelope
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

        public void SetEnvelope(string[] value)
        {
            var numberFormat = new CultureInfo("ja-JP").NumberFormat;
            Envelope = new Envelope
            {
                p1 = double.Parse(value[0], numberFormat),
                p2 = double.Parse(value[1], numberFormat),
                p3 = double.Parse(value[2], numberFormat),
                v1 = double.Parse(value[3], numberFormat),
                v2 = double.Parse(value[4], numberFormat),
                v3 = double.Parse(value[5], numberFormat),
                v4 = double.Parse(value[6], numberFormat),
                // 7 -- %
                p4 = value.Length > 7 ? double.Parse(value[8]) : 0,
                p5 = value.Length > 9 ? double.Parse(value[9], numberFormat) : 0,
                v5 = value.Length > 9 ? double.Parse(value[10], numberFormat) : 100
            };
        }

        public void SetVibrato(VibratoExpression value)
        {
            Vibrato = value;
        }

        public void SetVibrato(string vbr)
        {
            var cultureInfo = new CultureInfo("ja-JP");
            var value = vbr.Split(',');
            var vibrato = new VibratoExpression();
            if (value.Count() >= 7)
            {
                vibrato.Length = double.Parse(value[0], cultureInfo);
                vibrato.Period = double.Parse(value[1], cultureInfo);
                vibrato.Depth = double.Parse(value[2], cultureInfo);
                vibrato.In = double.Parse(value[3], cultureInfo);
                vibrato.Out = double.Parse(value[4], cultureInfo);
                vibrato.Shift = double.Parse(value[5], cultureInfo);
                vibrato.Drift = double.Parse(value[6], cultureInfo);
            }

            Vibrato = vibrato;
        }

        public void SetNoteControl(NoteControl noteControl)
        {
            noteControl.note = this;
            NoteControl = noteControl;
            SetNewLyric(Lyric);
        }

        #region private

        private Oto GetDefaultOto(string alias = "")
        {
            var phoneme = new Oto
            {
                Alias = alias,
                Offset = 0,
                Consonant = 0,
                Cutoff = 0,
                Preutter = 0,
                Overlap = 0,
                File = ""
            };
            return phoneme;
        }

        #endregion

    }
}