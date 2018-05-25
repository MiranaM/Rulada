using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PianoRoll.Util;

namespace PianoRoll.Model
{
    public abstract class UExpression
    {
        public UExpression(UNote parent, string name, string abbr) { _parent = parent; _name = name; _abbr = abbr; }

        protected UNote _parent;
        protected string _name;
        protected string _abbr;

        public UNote Parent { get { return _parent; } }
        public virtual string Name { get { return _name; } }
        public virtual string Abbr { get { return _abbr; } }

        public abstract string Type { get; }
        public abstract object Data { set; get; }

        public abstract UExpression Clone(UNote newParent);
        public abstract UExpression Split(UNote newParent, int offset);
    }

    public class PitchBendExpression : UExpression
    {
        public PitchBendExpression(UNote parent) : base(parent, "pitch", "PIT")
        {
            _data.Add(new PitchPoint(0, 0));
            _data.Add(new PitchPoint(0, 0));
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
        public override UExpression Clone(UNote newParent)
        {
            var data = new List<PitchPoint>();
            foreach (var p in this._data) data.Add(p.Clone());
            return new PitchBendExpression(newParent) { Data = data };
        }
        public override UExpression Split(UNote newParent, int offset)
        {
            var newdata = new List<PitchPoint>();
            while (_data.Count > 0 && _data.Last().X >= offset) { newdata.Add(_data.Last()); _data.Remove(_data.Last()); }
            newdata.Reverse();
            return new PitchBendExpression(newParent) { Data = newdata, SnapFirst = true };
        }
    }

    public class VibratoExpression : UExpression
    {
        public VibratoExpression(UNote parent) : base(parent, "vibrato", "VBR") { }
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
        public override UExpression Clone(UNote newParent)
        {
            return new VibratoExpression(newParent)
            {
                _length = _length,
                _period = _period,
                _depth = _depth,
                _in = _in,
                _out = _out,
                _shift = _shift,
                _drift = _drift
            };
        }
        public override UExpression Split(UNote newParent, int postick) { var exp = Clone(newParent); return exp; }
    }

    public class PitchPointHitTestResult
    {
        public UNote Note;
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

    class UPitch
    {
        public static void PitchFromUst(dynamic data, ref UNote note)
        {
            if (!data.ContainsKey("PBS"))
            {
                data["PBS"] = "-25";
                data["PBW"] = "50";
            }
            string pbs = "";
            note.PitchBend = new PitchBendExpression(note);
            var pts = note.PitchBend.Data as List<PitchPoint>;
            pts.Clear();
            pbs = data["PBS"];
            // PBS
            if (pbs.Contains(';'))
            {
                pts.Add(new PitchPoint(double.Parse(pbs.Split(new[] { ';' })[0]), double.Parse(pbs.Split(new[] { ';' })[1])));
                note.PitchBend.SnapFirst = false;
            }
            else
            {
                pts.Add(new PitchPoint(double.Parse(pbs), 0));
                note.PitchBend.SnapFirst = true;
            }

            double x = pts.First().X;
            if (data.ContainsKey("PBW"))
            {
                string[] w  = (data["PBW"]).GetType() == typeof(string) ? new string[] {data["PBW"]} : data["PBW"];
                //string[] w = pbw.Split(new[] { ',' });
                string[] y = null;
                if (data.ContainsKey("PBY"))
                {
                    y = (data["PBY"]).GetType() == typeof(string) ? new string[] { data["PBY"] } : data["PBY"];
                }
                // if (w.Count() > 1) y = pby.Split(new[] { ',' });
                for (int l = 0; l < w.Count() - 1; l++)
                {
                    x += w[l] == "" ? 0 : float.Parse(w[l]);
                    pts.Add(new PitchPoint(x, y[l] == "" ? 0 : double.Parse(y[l])));
                }
                pts.Add(new PitchPoint(x + double.Parse(w[w.Count() - 1]), 0));

                if (data.ContainsKey("PBM"))
                {
                    string[] m = (data["PBM"]).GetType() == typeof(string) ? new string[] { data["PBM"] } : data["PBM"];
                    for (int l = 0; l < m.Count() - 1; l++)
                    {
                        pts[l].Shape = m[l] == "r" ? PitchPointShape.o :
                                        m[l] == "s" ? PitchPointShape.l :
                                        m[l] == "j" ? PitchPointShape.l : PitchPointShape.io;
                    }
                }
            }
        }

        public static VibratoExpression VibratoFromUst(string StringData, UNote note)
        {
            VibratoExpression vibrato = note.Vibrato;
            var args = StringData.Split(new[] { ',' }).Select(double.Parse).ToList();
            if (args.Count() >= 7)
            {
                vibrato.Length = args[0];
                vibrato.Period = args[1];
                vibrato.Depth = args[2];
                vibrato.In = args[3];
                vibrato.Out = args[4];
                vibrato.Shift = args[5];
                vibrato.Drift = args[6];
            }
            return vibrato;
        }

        private static double InterpolateVibrato(VibratoExpression vibrato, double posMs)
        {
            double lengthMs = vibrato.Length / 100 * Ust.TickToMillisecond(vibrato.Parent.Length);
            double inMs = lengthMs * vibrato.In / 100;
            double outMs = lengthMs * vibrato.Out / 100;

            double value = -Math.Sin(2 * Math.PI * (posMs / vibrato.Period + vibrato.Shift / 100)) * vibrato.Depth;

            if (posMs < inMs) value *= posMs / inMs;
            else if (posMs > lengthMs - outMs) value *= (lengthMs - posMs) / outMs;

            return value;
        }

        public static int[] BuildPitchData(UNote note)
        {

            List<int> pitches = new List<int>();
            UNote prevNote = Ust.GetPrevNote(note);
            UNote nextNote = Ust.GetNextNote(note);
            // Get relevant pitch points
            List<PitchPoint> pps = new List<PitchPoint>();

            // what?
            bool prevNoteInvolved = prevNote != null; // && phoneme.Overlapped;
            bool nextNoteInvolved = nextNote != null; // && nextNote.Phonemes[0].Overlapped;

            double lastVibratoStartMs = 0;
            double lastVibratoEndMs = 0;
            double vibratoStartMs = 0;
            double vibratoEndMs = 0;

            if (prevNoteInvolved)
            {
                double offsetMs = Ust.TickToMillisecond(note.AbsoluteTime - prevNote.AbsoluteTime);
                foreach (PitchPoint pp in prevNote.PitchBend.Points)
                {
                    var newpp = pp.Clone();
                    newpp.X -= offsetMs;
                    newpp.Y -= (note.NoteNum - prevNote.NoteNum) * 10;
                    pps.Add(newpp);
                }
                if (prevNote.Vibrato.Depth != 0)
                {
                    lastVibratoStartMs = -Ust.TickToMillisecond(prevNote.Length) * prevNote.Vibrato.Length / 100;
                    lastVibratoEndMs = 0;
                }
            }

            foreach (PitchPoint pp in note.PitchBend.Points) pps.Add(pp);
            if (note.Vibrato.Depth != 0)
            {
                vibratoEndMs = Ust.TickToMillisecond(note.Length);
                vibratoStartMs = vibratoEndMs * (1 - note.Vibrato.Length / 100);
            }

            if (nextNoteInvolved)
            {
                double offsetMs = Ust.TickToMillisecond(note.AbsoluteTime - nextNote.AbsoluteTime);
                foreach (PitchPoint pp in nextNote.PitchBend.Points)
                {
                    var newpp = pp.Clone();
                    newpp.X -= offsetMs;
                    newpp.Y -= (note.NoteNum - nextNote.NoteNum) * 10;
                    pps.Add(newpp);
                }
            }

            double startMs = -note.Oto.Preutter;
            double endMs = Ust.TickToMillisecond(note.Length) -
                (nextNote != null ? nextNote.Oto.Preutter - nextNote.Oto.Preutter : 0);
            //double startMs = Ust.TickToMillisecond(note.AbsoluteTime) - note.Oto.Preutter;
            //double endMs = Ust.TickToMillisecond(note.Length + note.AbsoluteTime) -
            //    (nextNote != null ? nextNote.Oto.Preutter - nextNote.Oto.Overlap : 0);
                // (nextNote != null && nextNote.Phonemes[0].Overlapped ? nextNote.Phonemes[0].Preutter - nextNote.Phonemes[0].Overlap : 0);
            if (pps.Count > 0)
            {
                if (pps.First().X > startMs) pps.Insert(0, new PitchPoint(startMs, pps.First().Y));
                if (pps.Last().X < endMs) pps.Add(new PitchPoint(endMs, pps.Last().Y));
            }
            else
            {
                throw new Exception("Zero pitch points.");
                //return "\"\"";
            }

            // Interpolation
            double currMs = startMs;
            int i = 0;

            while (currMs < endMs)
            {
                //while (pps[i + 1].X < currMs - 1) i++;
                while (pps[i + 1].Y == pps[i].Y && i < pps.Count() - 2) i++;
                double pit = MusicMath.InterpolateShape(pps[i].X, pps[i + 1].X, pps[i].Y, pps[i + 1].Y, currMs, pps[i].Shape);
                //pit *= 10;

                // Apply vibratos
                if (currMs < lastVibratoEndMs && currMs >= lastVibratoStartMs)
                    pit += InterpolateVibrato(prevNote.Vibrato, currMs - lastVibratoStartMs);

                if (currMs < vibratoEndMs && currMs >= vibratoStartMs)
                    pit += InterpolateVibrato(note.Vibrato, currMs - vibratoStartMs);

                pitches.Add((int)pit);
                currMs += Settings.IntervalMs;
            }

            return pitches.ToArray();
        }

        public static void BuildPitchData2(UNote note)
        {

            List<int> pitches = new List<int>();
            List<PitchPoint> pps = new List<PitchPoint>();

            foreach (PitchPoint pp in note.PitchBend.Points) pps.Add(pp);

            // end and start points
            double startMs = pps.First().X < -note.Oto.Preutter ? pps.First().X : - note.Oto.Preutter;
            double endMs = Ust.TickToMillisecond(note.Length);

            // if there is notePrev, I change first point Y
            UNote prevNote = Ust.GetPrevNote(note);
            UNote nextNote = Ust.GetNextNote(note);
            // 1 halftone value
            int val = 10;
            if (prevNote != null && !prevNote.IsRest) pps.First().Y = (prevNote.NoteNum - note.NoteNum) * val;


            // if not all the length involved, add end and/or start pitch point
            if (pps.Count > 0)
            {
                if (pps.First().X > startMs) pps.Insert(0, new PitchPoint(startMs, pps.First().Y));
                if (pps.Last().X < endMs) pps.Add(new PitchPoint(endMs, pps.Last().Y));
            }
            else
            {
                throw new Exception("Zero pitch points.");
            }

            // Interpolation
            int i = -1;

            // up or down
            int dir = -222;
            // point value
            double y = -9999;
            // sin width
            double xk = -9991;
            // sin height
            double yk = -9990;
            // normalize to zero
            double C = -9990;
            // multiplayer
            double m = 10;
            foreach (PitchPoint pp in pps) pp.Y *= m;

            Console.WriteLine($"New note");
            foreach (PitchPoint pp in pps) Console.WriteLine($"y:{pp.Y}");

            for (double x = startMs; x <= endMs; x += Settings.IntervalMs)
            {
                // only S shape is allowed
                if (Math.Ceiling(pps[i+1].X / Settings.IntervalMs) == Math.Ceiling(x / Settings.IntervalMs) 
                                    && (i + 1) < pps.Count - 1)
                {
                    // goto next pitch points pair
                    i++;
                    xk = pps[i + 1].X - pps[i].X;
                    yk = pps[i + 1].Y - pps[i].Y;
                    dir = pps[i + 1].Y > pps[i].Y ? 1 : -1;
                    dir = pps[i + 1].Y == pps[i].Y ? 0 : dir;
                    C = pps[i + 1].Y;
                }
                // local x
                double xl = x - pps[i].X;
                // y
                y = -yk * ( 0.5 * Math.Cos((1 / xk) * 10 * xl / Math.PI) + 0.5) + C;

                pitches.Add((int)Math.Round(y));
                //Console.WriteLine($"[{x}; {y}] where X are {(int)((endMs - startMs) / Settings.IntervalMs)}, current={(int)((endMs - x) / Settings.IntervalMs)}, i={i}");
                if ((int)((endMs - x) / Settings.IntervalMs) < 0) throw new Exception("wut");
            }

            if (pitches == null) throw new Exception("Так блет а где питч");
            note.PitchBend.Array = pitches.ToArray();
        }
    }
}
