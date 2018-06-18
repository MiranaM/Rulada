using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public UExpression(string name, string abbr) { _name = name; _abbr = abbr; }

        //protected UNote _parent;
        protected string _name;
        protected string _abbr;

        //public UNote Parent { get { return _parent; } }
        public virtual string Name { get { return _name; } }
        public virtual string Abbr { get { return _abbr; } }

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
        public override string Type { get { return "pitch"; } }
        public override object Data { set { _data = (List<PitchPoint>)value; } get { return _data; } }
        public List<PitchPoint> Points { get { return _data; } }
        public bool SnapFirst { set { _snapFirst = value; } get { return _snapFirst; } }
        public void AddPoint(PitchPoint p) { _data.Add(p); _data.Sort(); }
        public void RemovePoint(PitchPoint p) { _data.Remove(p); }
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
        public VibratoExpression() : base("vibrato", "VBR") { }
        double _length;
        double _period;
        double _depth;
        double _in;
        double _out;
        double _shift;
        double _drift;
        public double Length { set { _length = Math.Max(0, Math.Min(100, value)); } get { return _length; } }
        public double Period { set { _period = Math.Max(64, Math.Min(512, value)); } get { return _period; } }
        public double Depth { set { _depth = Math.Max(5, Math.Min(200, value)); } get { return _depth; } }
        public double In { set { _in = Math.Max(0, Math.Min(100, value)); _out = Math.Min(_out, 100 - value); } get { return _in; } }
        public double Out { set { _out = Math.Max(0, Math.Min(100, value)); _in = Math.Min(_in, 100 - value); } get { return _out; } }
        public double Shift { set { _shift = Math.Max(0, Math.Min(100, value)); } get { return _shift; } }
        public double Drift { set { _drift = Math.Max(-100, Math.Min(100, value)); } get { return _drift; } }
        public override string Type { get { return "pitch"; } }
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
            if (this.X > other.X) return 1;
            else if (this.X == other.X) return 0;
            else return -1;
        }
        public ExpPoint(double x, double y) { X = x; Y = y; }
        public ExpPoint Clone() { return new ExpPoint(X, Y); }
    }

    public class PitchPoint : ExpPoint
    {
        public PitchPointShape Shape;
        public PitchPoint(double x, double y, PitchPointShape shape = PitchPointShape.io) : base(x, y) { Shape = shape; }
        public new PitchPoint Clone() { return new PitchPoint(X, Y, Shape); }
    }

    public enum PitchPointShape
    {
        /// <summary>
        /// SineInOut
        /// </summary>
        io,
        /// <summary>
        /// Linear
        /// </summary>
        l,
        /// <summary>
        /// SineIn
        /// </summary>
        i,
        /// <summary>
        /// SineOut
        /// </summary>
        o
    };

    public static class Pitch
    {
        public static void PitchFromUst(USTPitchData data, ref Note note)
        {
            if (data.PBS == "")
            {
                data.PBS = "-25";
                data.PBS = "50";
            }
            string pbs = "";
            note.PitchBend = new PitchBendExpression();
            var pts = note.PitchBend.Data as List<PitchPoint>;
            pts.Clear();
            pbs = data.PBS;
            // PBS
            if (pbs.Contains(';'))
            {
                var v1 = double.Parse(pbs.Split(new[] { ';' })[0], new CultureInfo("ja-JP"));
                var v2 = double.Parse(pbs.Split(new[] { ';' })[1], new CultureInfo("ja-JP"));
                pts.Add(new PitchPoint(v1, v2));
                note.PitchBend.SnapFirst = false;
            }
            else
            {
                pts.Add(new PitchPoint(double.Parse(pbs), 0));
                note.PitchBend.SnapFirst = true;
            }

            double x = pts.First().X;
            if (data.PBW != "")
            {
                string[] w  = data.PBW.Split(new[] { ',' });
                string[] y = null;
                if (w.Count() > 1) y = data.PBY.Split(new[] { ',' });
                for (int l = 0; l < w.Count() - 1; l++)
                {
                    x += w[l] == "" ? 0 : float.Parse(w[l]);
                    pts.Add(new PitchPoint(x, y[l] == "" ? 0 : double.Parse(y[l])));
                }
                pts.Add(new PitchPoint(x + double.Parse(w[w.Count() - 1]), 0));

                if (data.PBM != "")
                {
                    string[] m = data.PBM.Split(new[] { ',' });
                    for (int l = 0; l < m.Count() - 1; l++)
                    {
                        pts[l].Shape = m[l] == "r" ? PitchPointShape.o :
                                        m[l] == "s" ? PitchPointShape.l :
                                        m[l] == "j" ? PitchPointShape.l : PitchPointShape.io;
                    }
                }
            }
        }

        private static double InterpolateVibrato(VibratoExpression vibrato, double posMs, long noteLength)
        {
            double lengthMs = vibrato.Length / 100 * MusicMath.TickToMillisecond(noteLength);
            double inMs = lengthMs * vibrato.In / 100;
            double outMs = lengthMs * vibrato.Out / 100;

            double value = -Math.Sin(2 * Math.PI * (posMs / vibrato.Period + vibrato.Shift / 100)) * vibrato.Depth;

            if (posMs < inMs) value *= posMs / inMs;
            else if (posMs > lengthMs - outMs) value *= (lengthMs - posMs) / outMs;

            return value;
        }

        public static void BuildPitchData(Note note)
        {
            BuildVibratoInfo(note, out VibratoInfo vibratoInfo, out VibratoInfo vibratoPrevInfo);
            int[] pitches = BuildVibrato(vibratoInfo, vibratoPrevInfo);
            BuildPitchInfo(note, out PitchInfo pitchInfo);
            int[] pitchesP = BuildPitch(pitchInfo);
            if (pitchInfo.Start > 0) throw new Exception();
            int offset = -pitchInfo.Start / Settings.IntervalTick;
            pitches = Interpolate(pitchesP, pitches, offset);
            note.PitchBend.Array = pitches;
        }

        public static int SnapTick(int tick)
        {
            tick =  ((int)(tick + Settings.IntervalTick * 0.25) / Settings.IntervalTick * Settings.IntervalTick);
            if (tick % Settings.IntervalTick != 0)
                throw new Exception();
            return tick;
        }

        public static double SnapMs(double ms)
        {
            var tick = MusicMath.MillisecondToTick(ms);
            tick = SnapTick(tick);
            return MusicMath.MillisecondToTick(tick);

        }

        public static void BuildPitchInfo(Note note, out PitchInfo pitchInfo)
        {
            Note prevNote = note.GetPrev();
            Note nextNote = note.GetNext();
            Phoneme phoneme = note.Phoneme;
            List<PitchPoint> pps = new List<PitchPoint>();
            foreach (PitchPoint pp in note.PitchBend.Points) pps.Add(pp);
            if (pps.Count == 0)
            {
                int offsetY = prevNote == null ? -10 : MusicMath.GetYOffset(note, prevNote);
                pps.Add(new PitchPoint(MusicMath.TickToMillisecond(-40), offsetY));
                pps.Add(new PitchPoint(MusicMath.TickToMillisecond(40), 0));
            }

            // end and start ms
            double startMs = pps.First().X < -phoneme.Preutter ? pps.First().X : -phoneme.Preutter;
            double endMs = MusicMath.TickToMillisecond(note.Length);

            // 1 halftone value
            int val = 10;
            if (prevNote != null && note.IsConnectedLeft()) pps.First().Y = (prevNote.NoteNum - note.NoteNum) * val;

            // if not all the length involved, add end and/or start pitch points
            if (pps.First().X > startMs) pps.Insert(0, new PitchPoint(startMs, pps.First().Y));
            if (pps.Last().X < endMs) pps.Add(new PitchPoint(endMs, pps.Last().Y));

            var start = SnapTick(MusicMath.MillisecondToTick(pps.First().X));
            var end = SnapTick(MusicMath.MillisecondToTick(pps.Last().X));

            // combine all
            pitchInfo.Start = start;
            pitchInfo.End = end;
            pitchInfo.PitchPoints = pps.ToArray();
        }

        public static void BuildVibratoInfo(Note note, out VibratoInfo vibratoInfo, out VibratoInfo vibratoPrevInfo)
        {
            Note prevNote = note.GetPrev();
            Note nextNote = note.GetNext();
            vibratoInfo = new VibratoInfo()
            {
                Start = 0,
                End = 0,
                Vibrato = note.Vibrato,
                Length = note.Length
            };
            if (prevNote != null)
            {
                vibratoPrevInfo = new VibratoInfo()
                {
                    Start = 0,
                    End = 0,
                    Vibrato = prevNote.Vibrato,
                    Length = prevNote.Length
                };
            }
            else
            {
                vibratoPrevInfo = new VibratoInfo()
                {
                    Start = 0,
                    End = 0,
                    Vibrato = null,
                    Length = 0
                };
            }
            if (note.Vibrato != null && note.Vibrato.Depth != 0)
            {
                vibratoInfo.End = MusicMath.TickToMillisecond(note.Length);
                vibratoInfo.Start = vibratoInfo.End * (1 - note.Vibrato.Length / 100);
            }
            if (prevNote != null && prevNote.Vibrato != null && prevNote.Vibrato.Depth != 0)
            {
                vibratoPrevInfo.Start = -MusicMath.TickToMillisecond(prevNote.Length) * prevNote.Vibrato.Length / 100;
                vibratoPrevInfo.End = 0;
            }
        }

        public static int[] BuildPitch(PitchInfo pitchInfo)
        {
            List<int> pitches = new List<int>();
            int interv = Settings.IntervalTick;
            int i = -1;
            double xl = 0; // x local
            int dir = -99; // up or down
            double y = -99; // point value
            double xk = -99; // sin width
            double yk = -99; // sin height
            double C = -99; // normalize to zero
            bool IsNextPointTime;
            bool IsLastPoint;
            int nextX;
            int thisX;
            for (int x = pitchInfo.Start; x <= pitchInfo.End; x += interv)
            {
                // only S shape is allowed
                nextX = (int)(pitchInfo.PitchPoints[i + 1].X);
                thisX = (int)x;
                IsNextPointTime = thisX < 0 ? thisX + interv >= nextX : thisX + interv >= nextX;
                IsLastPoint = (i + 2) == pitchInfo.PitchPoints.Length;
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
                    nextX = (int)(pitchInfo.PitchPoints[i + 1].X / interv - 0.5 * Settings.IntervalMs);
                    nextX -= (int)(nextX % Settings.IntervalMs);
                    thisX = (int)Math.Ceiling((double)x / interv);
                    IsNextPointTime = thisX >= nextX;
                    IsLastPoint = (i + 2) == pitchInfo.PitchPoints.Length;
                }
                if (dir == -99) throw new Exception();
                yk = Math.Round(yk, 3);
                var X = Math.Abs(xk) < 5 ? 0 : (1 / xk) * 10 * xl / Math.PI;
                y = -yk * (0.5 * Math.Cos(X) + 0.5) + C;
                y *= 10;
                pitches.Add((int)Math.Round(y));
                xl += interv;
            }
            //if (i < pitchInfo.PitchPoints.Length - 2)
            //    throw new Exception("Some points was not processed");
            return pitches.ToArray();
        }

        public static int[] BuildVibrato(VibratoInfo vibratoInfo, VibratoInfo vibratoPrevInfo)
        {
            List<int> pitches = new List<int>();
            int interv = Settings.IntervalTick;
            for (int x = 0; x <= vibratoInfo.Length; x += interv)
            {
                double y = 0;
                //Apply vibratos
                if (MusicMath.TickToMillisecond(x) < vibratoPrevInfo.End && MusicMath.TickToMillisecond(x) >= vibratoPrevInfo.Start && vibratoPrevInfo.Vibrato != null)
                    y += InterpolateVibrato(vibratoPrevInfo.Vibrato, MusicMath.TickToMillisecond(x) - vibratoPrevInfo.Start, vibratoPrevInfo.Length);

                if (MusicMath.TickToMillisecond(x) < vibratoInfo.End && MusicMath.TickToMillisecond(x) >= vibratoInfo.Start)
                    y += InterpolateVibrato(vibratoInfo.Vibrato, MusicMath.TickToMillisecond(x) - vibratoInfo.Start, vibratoInfo.Length);

                pitches.Add((int)Math.Round(y));
            }
            return pitches.ToArray();
        }

        public static int[] Interpolate(int[] pitches1, int[] pitches2, int offset = 0)
        {
            List<int> pitches = new List<int>();
            int len = pitches1.Length > pitches2.Length + offset ? pitches1.Length : pitches2.Length;
            for (int i = -offset; i < len; i++)
            {
                int y1 = 0;
                int y2 = 0;
                if (pitches1.Length > i + offset && i + offset >= 0) y1 = pitches1[i + offset]; 
                if (pitches2.Length > i && i >= 0) y2 = pitches2[i];
                int z = y1 + y2;
                pitches.Add(z);
            }
            return pitches.ToArray();
        }

        public static void AveragePitch(Note note, Note noteNext)
        {
            BuildPitchInfo(note, out PitchInfo pitchInfo);
            BuildPitchInfo(noteNext, out PitchInfo pitchNextInfo);
            int[] thisPitch = note.PitchBend.Array;
            int[] nextPitch = noteNext.PitchBend.Array;
            int length = (int) SnapTick(-pitchNextInfo.Start / Settings.IntervalTick);
            int start = thisPitch.Length - length;
            if (start <= 0) return;
            int C = MusicMath.GetYOffset(note, noteNext);
            for (int k = 0; k < length; k++)
            {
                int x1 = k + start;
                int x2 = k;
                if (k + start >= thisPitch.Length) x1 = thisPitch.Length - 1;
                if (k >= nextPitch.Length) x2 = nextPitch.Length - 1;
                int y1 = thisPitch[x1];
                int y2 = nextPitch[x2];
                int z = y1 + y2;
                thisPitch[x1] = z - C;
                nextPitch[x2] = z;
            }
        }
    }
}
