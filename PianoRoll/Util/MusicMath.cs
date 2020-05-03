using System;
using System.Linq;
using PianoRoll.Control;
using PianoRoll.Model;

namespace PianoRoll.Util
{
    public static class MusicMath
    {
        public static string[] noteStrings =
            new string[12] {"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"};

        public static string GetNoteString(int noteNum)
        {
            return noteNum < 0 ? "" : noteStrings[noteNum % 12] + (noteNum / 12 - 1);
        }

        public static int[] BlackNoteNums = {1, 3, 6, 8, 10};

        public static bool IsBlackKey(int noteNum)
        {
            return BlackNoteNums.Contains(noteNum % 12);
        }

        public static bool IsCenterKey(int noteNum)
        {
            return noteNum % 12 == 0;
        }

        public static double[] zoomRatios = {4.0, 2.0, 1.0, 1.0 / 2, 1.0 / 4, 1.0 / 8, 1.0 / 16, 1.0 / 32, 1.0 / 64};

        public static double getZoomRatio(double quarterWidth, int beatPerBar, int beatUnit, double minWidth)
        {
            int i;

            switch (beatUnit)
            {
                case 2:
                    i = 0;
                    break;
                case 4:
                    i = 1;
                    break;
                case 8:
                    i = 2;
                    break;
                case 16:
                    i = 3;
                    break;
                default:
                    throw new Exception("Invalid beat unit.");
            }

            if (beatPerBar % 4 == 0) i--; // level below bar is half bar, or 2 beatunit
            // else // otherwise level below bar is beat unit

            if (quarterWidth * beatPerBar * 4 <= minWidth * beatUnit) return beatPerBar / beatUnit * 4;

            while (i + 1 < zoomRatios.Length && quarterWidth * zoomRatios[i + 1] > minWidth) i++;
            return zoomRatios[i];
        }

        public static double TickToMillisecond(double tick)
        {
            double BeatTicks = Settings.Resolution;
            var TempoCoeff = 60 / Project.Tempo;
            var size = tick / BeatTicks;
            return size * TempoCoeff * 1000;
        }

        public static int MillisecondToTick(double ms)
        {
            double BeatTicks = Settings.Resolution;
            var TempoCoeff = 60 / Project.Tempo;
            return (int) (ms / TempoCoeff / 1000 * BeatTicks);
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

        public static double InterpolateShape(double x0, double x1, double y0, double y1, double x,
            PitchPointShape shape)
        {
            switch (shape)
            {
                case PitchPointShape.io:
                    return SinEasingInOut(x0, x1, y0, y1, x);
                case PitchPointShape.i:
                    return SinEasingIn(x0, x1, y0, y1, x);
                case PitchPointShape.o:
                    return SinEasingOut(x0, x1, y0, y1, x);
                default:
                    return Linear(x0, x1, y0, y1, x);
            }
        }

        public static double InterpolateShapeX(double x0, double x1, double y0, double y1, double y,
            PitchPointShape shape)
        {
            switch (shape)
            {
                case PitchPointShape.io:
                    return SinEasingInOutX(x0, x1, y0, y1, y);
                case PitchPointShape.i:
                    return SinEasingInX(x0, x1, y0, y1, y);
                case PitchPointShape.o:
                    return SinEasingOutX(x0, x1, y0, y1, y);
                default:
                    return LinearX(x0, x1, y0, y1, y);
            }
        }

        public static double DecibelToLinear(double db)
        {
            return Math.Pow(10, db / 20);
        }

        public static double LinearToDecibel(double v)
        {
            return Math.Log10(v) * 20;
        }

        public static string NoteNum2String(int noteNum)
        {
            var octave = noteNum / 12 + 1;
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

        /// <summary>
        ///     NoteNum to Y Position
        /// </summary>
        /// <param name="noteNumber"></param>
        /// <returns></returns>
        public static double GetNoteYPosition(int noteNumber)
        {
            return (Settings.Octaves * 12 - 1 - noteNumber) * PartEditor.yScale;
        }

        /// <summary>
        ///     Y Position to NoteNum
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int GetNoteNum(double y)
        {
            return (int) (Settings.Octaves * 12 - y / PartEditor.yScale);
        }

        /// <summary>
        ///     X Position to Absolute Time
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static long GetAbsoluteTime(double x)
        {
            return (long) (x / PartEditor.xScale);
        }

        /// <summary>
        ///     Absolute Time to X Position
        /// </summary>
        /// <param name="absoluteTime"></param>
        /// <returns></returns>
        public static double GetNoteXPosition(long absoluteTime)
        {
            return absoluteTime * PartEditor.xScale;
        }

        /// <summary>
        ///     Y position between NoteNum points
        /// </summary>
        /// <param name="source"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static int GetYOffset(Note source, Note subject)
        {
            return (source.NoteNum - subject.NoteNum) * 100;
        }

        /// <summary>
        ///     Snap parameters like X Position ON EDIT
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double SnapX(double x)
        {
            return SnapTick((long) (x / PartEditor.xScale)) * PartEditor.xScale;
        }

        /// <summary>
        ///     Snap parameters like Length, AbsoluteTime etc ON EDIT
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public static double SnapAbsoluteTime(long tick)
        {
            tick = (int) (tick + Project.MinNoteLengthTick * 0.5);
            tick -= tick % Project.MinNoteLengthTick;
            return tick;
        }

        /// <summary>
        ///     Snap tick ON RENDER
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public static long SnapTick(long tick)
        {
            tick = (long) (tick + Settings.IntervalTick * 0.25) / Settings.IntervalTick * Settings.IntervalTick;
            if (tick % Settings.IntervalTick != 0) throw new Exception();
            return tick;
        }

        /// <summary>
        ///     Snap ms ON RENDER
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static double SnapMs(double ms)
        {
            var tick = MillisecondToTick(ms);
            tick = (int) SnapTick(tick);
            return MillisecondToTick(tick);
        }
    }
}