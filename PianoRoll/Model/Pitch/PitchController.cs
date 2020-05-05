using System;
using System.Collections.Generic;
using System.Linq;
using PianoRoll.Util;

namespace PianoRoll.Model.Pitch
{
    public class PitchController
    {
        #region singleton base

        private static PitchController current;
        private PitchController()
        {

        }

        public static PitchController Current
        {
            get
            {
                if (current == null)
                {
                    current = new PitchController();
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
            var autoPitchLength = 40;
            var autoPitchOffset = -20;
            var firstNoteRaise = 10;

            var prevNote = note.IsConnectedLeft() ? note.GetPrev() : null;
            var nextNote = note.IsConnectedRight() ? note.GetNext() : null;
            var oto = note.SafeOto;
            var pps = new List<PitchPoint>();
            foreach (var pp in note.PitchBend.Points)
                pps.Add(pp);
            if (pps.Count == 0)
            {
                var offsetY = prevNote == null ? -firstNoteRaise : GetPitchDiff(note.NoteNum, prevNote.NoteNum);
                pps.Add(new PitchPoint(MusicMath.Current.TickToMillisecond(autoPitchOffset), offsetY));
                pps.Add(new PitchPoint(MusicMath.Current.TickToMillisecond(autoPitchOffset + autoPitchLength), 0));
            }

            var cutoffFromNext = nextNote != null ? nextNote.StraightPre : 0;

            // end and start ms
            var startMs = pps.First().X < -oto.Preutter ? pps.First().X : -oto.Preutter;
            double endMs = MusicMath.Current.TickToMillisecond(note.Length) - cutoffFromNext;

            // 1 halftone value
            var val = 10;
            if (prevNote != null && note.IsConnectedLeft())
                pps.First().Y = (prevNote.NoteNum - note.NoteNum) * val;

            // if not all the length involved, add end and/or start pitch points
            if (pps.First().X > startMs)
                pps.Insert(0, new PitchPoint(startMs, pps.First().Y));
            if (pps.Last().X < endMs)
                pps.Add(new PitchPoint(endMs, pps.Last().Y));

            var start = (int) MusicMath.Current.SnapMs(pps.First().X);
            var end = (int) MusicMath.Current.SnapMs(pps.Last().X);

            // combine all
            pitchInfo.Start = start;
            pitchInfo.End = end;
            pitchInfo.PitchPoints = pps.ToArray();
        }

        public double GetPitchDiff(int noteNum1, int noteNum2)
        {
            return Math.Abs(noteNum1 - noteNum2) * 100;
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
    }
}