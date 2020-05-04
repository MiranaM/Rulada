﻿using System;
using System.Collections.Generic;
using System.Linq;
using PianoRoll.Util;

namespace PianoRoll.Model
{
    public struct PitchInfo
    {
        public PitchPoint[] PitchPoints;

        // pitch start tick
        public int Start;

        // pitch end tick
        public int End;
    }

    public struct VibratoInfo
    {
        // vibrato start ms
        public double Start;

        // vibrato end ms
        public double End;

        // note length tick
        public long Length;

        // vibrato
        public VibratoExpression Vibrato;
    }

    public abstract class UExpression
    {
        public UExpression(string name, string abbr)
        {
            _name = name;
            _abbr = abbr;
        }

        //protected UNote _parent;
        protected string _name;
        protected string _abbr;

        //public UNote Parent { get { return _parent; } }
        public virtual string Name => _name;

        public virtual string Abbr => _abbr;

        public abstract string Type { get; }
        public abstract object Data { set; get; }

        //public abstract UExpression Clone(UNote newParent);
        //public abstract UExpression Split(UNote newParent, int offset);
    }

    public class PitchBendExpression : UExpression
    {
        public PitchBendExpression() : base("pitch", "PIT")
        {
        }

        protected List<PitchPoint> _data = new List<PitchPoint>();
        protected bool _snapFirst = true;

        public override string Type => "pitch";

        public override object Data
        {
            set => _data = (List<PitchPoint>) value;
            get => _data;
        }

        public List<PitchPoint> Points => _data;

        public bool SnapFirst
        {
            set => _snapFirst = value;
            get => _snapFirst;
        }

        public void AddPoint(PitchPoint p)
        {
            _data.Add(p);
            _data.Sort();
        }

        public void RemovePoint(PitchPoint p)
        {
            _data.Remove(p);
        }

        public int[] Array { get; set; }
        //public override UExpression Clone(UNote newParent)
        //{
        //    var data = new List<PitchPoint>();
        //    foreach (var p in this._data) data.Add(p.Clone());
        //    return new PitchBendExpression(newParent) { Data = data };
        //}
        //public override UExpression Split(UNote newParent, int offset)
        //{
        //    var newdata = new List<PitchPoint>();
        //    while (_data.Count > 0 && _data.Last().X >= offset) { newdata.Add(_data.Last()); _data.Remove(_data.Last()); }
        //    newdata.Reverse();
        //    return new PitchBendExpression(newParent) { Data = newdata, SnapFirst = true };
        //}
    }

    public class VibratoExpression : UExpression
    {
        public VibratoExpression() : base("vibrato", "VBR")
        {
        }

        private double _length;
        private double _period;
        private double _depth;
        private double _in;
        private double _out;
        private double _shift;
        private double _drift;

        public double Length
        {
            set => _length = Math.Max(0, Math.Min(100, value));
            get => _length;
        }

        public double Period
        {
            set => _period = Math.Max(64, Math.Min(512, value));
            get => _period;
        }

        public double Depth
        {
            set => _depth = Math.Max(5, Math.Min(200, value));
            get => _depth;
        }

        public double In
        {
            set
            {
                _in = Math.Max(0, Math.Min(100, value));
                _out = Math.Min(_out, 100 - value);
            }
            get => _in;
        }

        public double Out
        {
            set
            {
                _out = Math.Max(0, Math.Min(100, value));
                _in = Math.Min(_in, 100 - value);
            }
            get => _out;
        }

        public double Shift
        {
            set => _shift = Math.Max(0, Math.Min(100, value));
            get => _shift;
        }

        public double Drift
        {
            set => _drift = Math.Max(-100, Math.Min(100, value));
            get => _drift;
        }

        public override string Type => "pitch";

        public override object Data { set; get; }
        //public override UExpression Clone(UNote newParent)
        //{
        //    return new VibratoExpression(newParent)
        //    {
        //        _length = _length,
        //        _period = _period,
        //        _depth = _depth,
        //        _in = _in,
        //        _out = _out,
        //        _shift = _shift,
        //        _drift = _drift
        //    };
        //}
        //public override UExpression Split(UNote newParent, int postick) { var exp = Clone(newParent); return exp; }
    }

    public class PitchPointHitTestResult
    {
        public Note Note;
        public int Index;
        public bool OnPoint;
        public double X;
        public double Y;
    }

    public class ExpPoint : IComparable<ExpPoint>
    {
        public double X;
        public double Y;

        public int CompareTo(ExpPoint other)
        {
            if (X > other.X)
                return 1;
            if (X == other.X)
                return 0;
            return -1;
        }

        public ExpPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public ExpPoint Clone()
        {
            return new ExpPoint(X, Y);
        }
    }

    public class PitchPoint : ExpPoint
    {
        public PitchPointShape Shape;

        public PitchPoint(double x, double y, PitchPointShape shape = PitchPointShape.io) : base(x, y)
        {
            Shape = shape;
        }

        public new PitchPoint Clone()
        {
            return new PitchPoint(X, Y, Shape);
        }
    }

    public enum PitchPointShape
    {
        /// <summary>
        ///     SineInOut
        /// </summary>
        io,

        /// <summary>
        ///     Linear
        /// </summary>
        l,

        /// <summary>
        ///     SineIn
        /// </summary>
        i,

        /// <summary>
        ///     SineOut
        /// </summary>
        o
    }

    public class Pitch
    {
        #region singleton base

        private static Pitch current;
        private Pitch()
        {

        }

        public static Pitch Current
        {
            get
            {
                if (current == null)
                {
                    current = new Pitch();
                }
                return current;
            }
        }

        #endregion


        private double InterpolateVibrato(VibratoExpression vibrato, double posMs, long noteLength)
        {
            var lengthMs = vibrato.Length / 100 * MusicMath.Current.TickToMillisecond(noteLength);
            var inMs = lengthMs * vibrato.In / 100;
            var outMs = lengthMs * vibrato.Out / 100;

            var value = -Math.Sin(2 * Math.PI * (posMs / vibrato.Period + vibrato.Shift / 100)) * vibrato.Depth;

            if (posMs < inMs)
                value *= posMs / inMs;
            else if (posMs > lengthMs - outMs) value *= (lengthMs - posMs) / outMs;

            return value;
        }

        public void BuildPitchData(Note note)
        {
            BuildVibratoInfo(note, out var vibratoInfo, out var vibratoPrevInfo);
            var pitches = BuildVibrato(vibratoInfo, vibratoPrevInfo);
            BuildPitchInfo(note, out var pitchInfo);
            var pitchesP = BuildPitch(pitchInfo);
            if (pitchInfo.Start > 0) throw new Exception();
            var offset = -pitchInfo.Start / Settings.Current.IntervalTick;
            pitches = Interpolate(pitchesP, pitches, offset);
            note.PitchBend.Array = pitches;
        }

        public void BuildPitchInfo(Note note, out PitchInfo pitchInfo)
        {
            var prevNote = note.IsConnectedLeft() ? note.GetPrev() : null;
            var nextNote = note.IsConnectedRight() ? note.GetNext() : null;
            var phoneme = note.HasPhoneme && note.Phoneme != null ? note.Phoneme : note.DefaultPhoneme;
            var pps = new List<PitchPoint>();
            foreach (var pp in note.PitchBend.Points) pps.Add(pp);
            if (pps.Count == 0)
            {
                var offsetY = prevNote == null ? -10 : MusicMath.Current.GetYOffset(note, prevNote);
                pps.Add(new PitchPoint(MusicMath.Current.TickToMillisecond(-40), offsetY));
                pps.Add(new PitchPoint(MusicMath.Current.TickToMillisecond(40), 0));
            }

            // end and start ms
            var startMs = pps.First().X < -phoneme.Preutter ? pps.First().X : -phoneme.Preutter;
            double endMs = MusicMath.Current.TickToMillisecond(note.Length);

            // 1 halftone value
            var val = 10;
            if (prevNote != null && note.IsConnectedLeft()) pps.First().Y = (prevNote.NoteNum - note.NoteNum) * val;

            // if not all the length involved, add end and/or start pitch points
            if (pps.First().X > startMs) pps.Insert(0, new PitchPoint(startMs, pps.First().Y));
            if (pps.Last().X < endMs) pps.Add(new PitchPoint(endMs, pps.Last().Y));

            var start = (int) MusicMath.Current.SnapMs(pps.First().X);
            var end = (int) MusicMath.Current.SnapMs(pps.Last().X);

            // combine all
            pitchInfo.Start = start;
            pitchInfo.End = end;
            pitchInfo.PitchPoints = pps.ToArray();
        }

        public void BuildVibratoInfo(Note note, out VibratoInfo vibratoInfo, out VibratoInfo vibratoPrevInfo)
        {
            var prevNote = note.GetPrev();
            var nextNote = note.GetNext();
            vibratoInfo = new VibratoInfo {Start = 0, End = 0, Vibrato = note.Vibrato, Length = note.Length};
            if (prevNote != null)
                vibratoPrevInfo = new VibratoInfo
                {
                    Start = 0, End = 0, Vibrato = prevNote.Vibrato, Length = prevNote.Length
                };
            else
                vibratoPrevInfo = new VibratoInfo {Start = 0, End = 0, Vibrato = null, Length = 0};

            if (note.Vibrato != null && note.Vibrato.Depth != 0)
            {
                vibratoInfo.End = MusicMath.Current.TickToMillisecond(note.Length);
                vibratoInfo.Start = vibratoInfo.End * (1 - note.Vibrato.Length / 100);
            }

            if (prevNote != null && prevNote.Vibrato != null && prevNote.Vibrato.Depth != 0)
            {
                vibratoPrevInfo.Start = -MusicMath.Current.TickToMillisecond(prevNote.Length) * prevNote.Vibrato.Length / 100;
                vibratoPrevInfo.End = 0;
            }
        }

        public int[] BuildPitch(PitchInfo pitchInfo)
        {
            var pitches = new List<int>();
            var interv = Settings.Current.IntervalTick;
            var i = -1;
            double xl = 0; // x local
            var dir = -99; // up or down
            double y = -99; // point value
            double xk = -99; // sin width
            double yk = -99; // sin height
            double C = -99; // normalize to zero
            bool IsNextPointTime;
            bool IsLastPoint;
            int nextX;
            int thisX;
            for (var x = pitchInfo.Start; x <= pitchInfo.End; x += interv)
            {
                // only S shape is allowed
                nextX = (int) pitchInfo.PitchPoints[i + 1].X;
                thisX = x;
                IsNextPointTime = thisX < 0 ? thisX + interv >= nextX : thisX + interv >= nextX;
                IsLastPoint = i + 2 == pitchInfo.PitchPoints.Length;
                while (IsNextPointTime && !IsLastPoint || i < 0)
                {
                    // goto next pitch points pair
                    i++;
                    xk = pitchInfo.PitchPoints[i + 1].X - pitchInfo.PitchPoints[i].X;
                    yk = pitchInfo.PitchPoints[i + 1].Y - pitchInfo.PitchPoints[i].Y;
                    dir = pitchInfo.PitchPoints[i + 1].Y > pitchInfo.PitchPoints[i].Y ? 1 : -1;
                    dir = pitchInfo.PitchPoints[i + 1].Y == pitchInfo.PitchPoints[i].Y ? 0 : dir;
                    C = pitchInfo.PitchPoints[i + 1].Y;
                    xl = 0;
                    nextX = (int) (pitchInfo.PitchPoints[i + 1].X / interv - 0.5 * Settings.Current.IntervalMs);
                    nextX -= (int) (nextX % Settings.Current.IntervalMs);
                    thisX = (int) Math.Ceiling((double) x / interv);
                    IsNextPointTime = thisX >= nextX;
                    IsLastPoint = i + 2 == pitchInfo.PitchPoints.Length;
                }

                if (dir == -99) throw new Exception();
                yk = Math.Round(yk, 3);
                var X = Math.Abs(xk) < 5 ? 0 : 1 / xk * 10 * xl / Math.PI;
                y = -yk * (0.5 * Math.Cos(X) + 0.5) + C;
                y *= 10;
                pitches.Add((int) Math.Round(y));
                xl += interv;
            }

            //if (i < pitchInfo.PitchPoints.Length - 2)
            //    throw new Exception("Some points was not processed");
            return pitches.ToArray();
        }

        public int[] BuildVibrato(VibratoInfo vibratoInfo, VibratoInfo vibratoPrevInfo)
        {
            var pitches = new List<int>();
            var interv = Settings.Current.IntervalTick;
            for (var x = 0; x <= vibratoInfo.Length; x += interv)
            {
                double y = 0;
                //Apply vibratos
                if (MusicMath.Current.TickToMillisecond(x) < vibratoPrevInfo.End &&
                    MusicMath.Current.TickToMillisecond(x) >= vibratoPrevInfo.Start && vibratoPrevInfo.Vibrato != null)
                    y += InterpolateVibrato(vibratoPrevInfo.Vibrato,
                        MusicMath.Current.TickToMillisecond(x) - vibratoPrevInfo.Start, vibratoPrevInfo.Length);

                if (MusicMath.Current.TickToMillisecond(x) < vibratoInfo.End &&
                    MusicMath.Current.TickToMillisecond(x) >= vibratoInfo.Start)
                    y += InterpolateVibrato(vibratoInfo.Vibrato, MusicMath.Current.TickToMillisecond(x) - vibratoInfo.Start,
                        vibratoInfo.Length);

                pitches.Add((int) Math.Round(y));
            }

            return pitches.ToArray();
        }

        public int[] Interpolate(int[] pitches1, int[] pitches2, int offset = 0)
        {
            var pitches = new List<int>();
            var len = pitches1.Length > pitches2.Length + offset ? pitches1.Length : pitches2.Length;
            for (var i = -offset; i < len; i++)
            {
                var y1 = 0;
                var y2 = 0;
                if (pitches1.Length > i + offset && i + offset >= 0) y1 = pitches1[i + offset];
                if (pitches2.Length > i && i >= 0) y2 = pitches2[i];
                var z = y1 + y2;
                pitches.Add(z);
            }

            return pitches.ToArray();
        }

        public void AveragePitch(Note note, Note noteNext)
        {
            if (!note.IsConnectedRight()) return;
            BuildPitchInfo(note, out var pitchInfo);
            BuildPitchInfo(noteNext, out var pitchNextInfo);
            var thisPitch = note.PitchBend.Array;
            var nextPitch = noteNext.PitchBend.Array;
            var length = (int)MusicMath.Current.SnapTick(-pitchNextInfo.Start / Settings.Current.IntervalTick);
            var start = thisPitch.Length - length;
            if (start <= 0) return;
            var C = MusicMath.Current.GetYOffset(note, noteNext);
            for (var k = 0; k < length; k++)
            {
                var x1 = k + start;
                var x2 = k;
                if (k + start >= thisPitch.Length) x1 = thisPitch.Length - 1;
                if (k >= nextPitch.Length) x2 = nextPitch.Length - 1;
                var y1 = thisPitch[x1];
                var y2 = nextPitch[x2];
                var z = y1 + y2;
                thisPitch[x1] = z - C;
                nextPitch[x2] = z;
            }
        }
    }
}