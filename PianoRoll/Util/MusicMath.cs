using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PianoRoll.Control;
using PianoRoll.Model;
using PianoRoll.View;

namespace PianoRoll.Util
{
    public static class MusicMath
    {
        public static string[] noteStrings = new String[12] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string GetNoteString(int noteNum) { return noteNum < 0 ? "" : noteStrings[noteNum % 12] + (noteNum / 12 - 1).ToString(); }

        public static int[] BlackNoteNums = { 1, 3, 6, 8, 10 };
        public static bool IsBlackKey(int noteNum) { return BlackNoteNums.Contains(noteNum % 12); }

        public static bool IsCenterKey(int noteNum) { return noteNum % 12 == 0; }

        public static double[] zoomRatios = { 4.0, 2.0, 1.0, 1.0 / 2, 1.0 / 4, 1.0 / 8, 1.0 / 16, 1.0 / 32, 1.0 / 64 };

        public static double getZoomRatio(double quarterWidth, int beatPerBar, int beatUnit, double minWidth)
        {
            int i;

            switch (beatUnit)
            {
                case 2: i = 0; break;
                case 4: i = 1; break;
                case 8: i = 2; break;
                case 16: i = 3; break;
                default: throw new Exception("Invalid beat unit.");
            }

            if (beatPerBar % 4 == 0) i--; // level below bar is half bar, or 2 beatunit
            // else // otherwise level below bar is beat unit

            if (quarterWidth * beatPerBar * 4 <= minWidth * beatUnit)
            {
                return beatPerBar / beatUnit * 4;
            }
            else
            {
                while (i + 1 < zoomRatios.Length && quarterWidth * zoomRatios[i + 1] > minWidth) i++;
                return zoomRatios[i];
            }
        }

        static double GetBeatTicks()
        {
            return Settings.Resolution;
        }

        static double GetTempoCoeff()
        {
            return 60 / Project.Tempo;
        }

        public static double TickToMillisecond(double tick)
        {
            double BeatTicks = GetBeatTicks();
            double TempoCoeff = GetTempoCoeff();
            double size = tick / BeatTicks;
            return size * TempoCoeff * 1000;
        }

        public static int MillisecondToTick(double ms)
        {
            double BeatTicks = GetBeatTicks();
            double TempoCoeff = GetTempoCoeff();
            return (int)(ms / TempoCoeff / 1000 * BeatTicks);
        }

        public static double SinEasingInOut(double x0, double x1, double y0, double y1, double x)
        {
            var z = y0 + (y1 - y0) * (1 - Math.Cos((x - x0) / (x1 - x0) * Math.PI)) / 2;
            return y0 + (y1 - y0) * (1 - Math.Cos((x - x0) / (x1 - x0) * Math.PI)) / 2;
        }

        public static double SinEasingInOutX(double x0, double x1, double y0, double y1, double y)
        {
            return Math.Acos(1 - (y - y0) * 2 / (y1 - y0)) / Math.PI * (x1 - x0) + x0;
        }

        public static double SinEasingIn(double x0, double x1, double y0, double y1, double x)
        {
            return y0 + (y1 - y0) * (1 - Math.Cos((x - x0) / (x1 - x0) * Math.PI / 2));
        }

        public static double SinEasingInX(double x0, double x1, double y0, double y1, double y)
        {
            return Math.Acos(1 - (y - y0) / (y1 - y0)) / Math.PI * 2 * (x1 - x0) + x0;
        }

        public static double SinEasingOut(double x0, double x1, double y0, double y1, double x)
        {
            return y0 + (y1 - y0) * Math.Sin((x - x0) / (x1 - x0) * Math.PI / 2);
        }

        public static double SinEasingOutX(double x0, double x1, double y0, double y1, double y)
        {
            return Math.Asin((y - y0) / (y1 - y0)) / Math.PI * 2 * (x1 - x0) + x0;
        }

        public static double Linear(double x0, double x1, double y0, double y1, double x)
        {
            return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
        }

        public static double LinearX(double x0, double x1, double y0, double y1, double y)
        {
            return (y - y0) / (y1 - y0) * (x1 - x0) + x0;
        }

        public static double InterpolateShape(double x0, double x1, double y0, double y1, double x, PitchPointShape shape)
        {
            switch (shape)
            {
                case PitchPointShape.io: return MusicMath.SinEasingInOut(x0, x1, y0, y1, x);
                case PitchPointShape.i: return MusicMath.SinEasingIn(x0, x1, y0, y1, x);
                case PitchPointShape.o: return MusicMath.SinEasingOut(x0, x1, y0, y1, x);
                default: return MusicMath.Linear(x0, x1, y0, y1, x);
            }
        }

        public static double InterpolateShapeX(double x0, double x1, double y0, double y1, double y, PitchPointShape shape)
        {
            switch (shape)
            {
                case PitchPointShape.io: return MusicMath.SinEasingInOutX(x0, x1, y0, y1, y);
                case PitchPointShape.i: return MusicMath.SinEasingInX(x0, x1, y0, y1, y);
                case PitchPointShape.o: return MusicMath.SinEasingOutX(x0, x1, y0, y1, y);
                default: return MusicMath.LinearX(x0, x1, y0, y1, y);
            }
        }

        public static double DecibelToLinear(double db) { return Math.Pow(10, db / 20); }

        public static double LinearToDecibel(double v) { return Math.Log10(v) * 20; }


        public static string NoteNum2String(int noteNum)
        {
            int octave = noteNum / 12 + 1;
            string note;
            switch (11 - noteNum % 12)
            {
                case 0:
                    note = "B";
                    break;
                case 1:
                    note = "A#";
                    break;
                case 2:
                    note = "A";
                    break;
                case 3:
                    note = "G#";
                    break;
                case 4:
                    note = "G";
                    break;
                case 5:
                    note = "F#";
                    break;
                case 6:
                    note = "F";
                    break;
                case 7:
                    note = "E";
                    break;
                case 8:
                    note = "D#";
                    break;
                case 9:
                    note = "D";
                    break;
                case 10:
                    note = "C#";
                    break;
                default:
                    note = "C";
                    break;
            }
            return $"{note}{octave}";
        }

        public static double GetNoteYPosition(int noteNumber)
        {
            return (double)(Settings.Octaves * 12 - 1 - noteNumber) * PartEditor.yScale;
        }

        public static int GetNoteNum(double y)
        {
            return (int)(Settings.Octaves * 12 - y / PartEditor.yScale);
        }

        public static long GetStartTime(double x)
        {
            return (long)(x / PartEditor.xScale);
        }

        public static double GetNoteXPosition(long startTime)
        {
            return (double)startTime * PartEditor.xScale;
        }


        public static int GetYOffset(Note source, Note subject)
        {
            return (source.NoteNum - subject.NoteNum) * 100;
        }


        public static double SnapX(double x)
        {
            return SnapTick((long)(x / PartEditor.xScale)) * PartEditor.xScale;
        }

        public static long SnapTick(long tick)
        {
            tick = ((long)(tick + Settings.IntervalTick * 0.25) / Settings.IntervalTick * Settings.IntervalTick);
            if (tick % Settings.IntervalTick != 0)
                throw new Exception();
            return tick;
        }

        public static double SnapMs(double ms)
        {
            var tick = MusicMath.MillisecondToTick(ms);
            tick = (int)SnapTick(tick);
            return MusicMath.MillisecondToTick(tick);
        }

    }
}
