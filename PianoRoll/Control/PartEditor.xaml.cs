using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.Midi;
using PianoRoll.Control;
using PianoRoll.Model;
using PianoRoll.View;
using PianoRoll.Util;
using System.Globalization;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для PartEditor.xaml
    /// </summary>
    public partial class PartEditor : UserControl
    {

        #region variables
        public Part Part;
        public static double xScale = 1.0 / 15;
        public static double yScale = 15;
        private long lastPosition;
        public int MaxDivider = 4;
        public bool doSnap = true;
        public int octaves = 7;
        private int minBars = 4;
        private double minWidth;
        public PitchBendExpression PitchBend;

        #endregion

        public PartEditor()
        {
            xScale = (80.0 / Settings.Resolution);
            minWidth = minBars * Project.BeatPerBar * Settings.Resolution;
            InitializeComponent();
            DrawInit();
        }

        public void Draw()
        {
            DrawNotes();
            Resize();
            CreateBackgroundCanvas();
            DrawGrid();
        }

        public void Resize()
        {
            RootCanvas.Width = lastPosition > minWidth ? lastPosition * xScale : minWidth * xScale;
            RootCanvas.Height = octaves * 12 * yScale;
        }

        public void Clear()
        {
            GridCanvas.Children.Clear();
            NoteCanvas.Children.Clear();
            NoteBackgroundCanvas.Children.Clear();
            DrawInit();
        }

        public void DrawInit()
        {
            lastPosition = Settings.Resolution * Project.BeatPerBar * minBars;
            Resize();
            DrawGrid();
            CreatePiano();
            CreateBackgroundCanvas();
        }

        public void DrawNotes()
        {
            if (Part == null) return;

            Part.RefreshPhonemes();
            NoteCanvas.Children.Clear();
            PitchCanvas.Children.Clear();
            PitchPointCanvas.Children.Clear();
            lastPosition = 0;

            foreach (Note note in Part.Notes)
            {
                NoteThumb noteControl = MakeNote(note.NoteNum, note.AbsoluteTime, note.Length, note.Lyric);
                lastPosition = Math.Max(lastPosition, lastPosition + note.Length);
                if (!note.IsRest)
                {
                    noteControl.note = note;
                    //noteControl.onUstChanged += DrawNotes;
                    noteControl.SetText(note.Lyric);
                    if (note.HasPhoneme)
                    {
                        noteControl.ToolTip = note.Phoneme.File;
                    }
                    else
                    {
                        noteControl.Background = new SolidColorBrush(System.Windows.Media.Colors.DarkOrange);
                        noteControl.ToolTip = "can't found source file";
                    }
                    note.NoteControl = noteControl;
                    NoteCanvas.Children.Add(noteControl);
                }


            }
            //scrollViewer.ScrollToVerticalOffset(540);
        }

        public void PitchOff()
        {
            PitchCanvas.Children.Clear();
            PitchPointCanvas.Children.Clear();
        }

        public void DrawPitch()
        {
            PitchOff();
            Part.BuildPitch();
            int i = 0;
            foreach (Note note in Part.Notes)
            {
                if (!note.HasPhoneme || note.IsRest) continue;
                double x0 = (double)note.NoteControl.GetValue(Canvas.LeftProperty);
                double y0 = (double)note.NoteControl.GetValue(Canvas.TopProperty) + yScale / 2;
                if (note.PitchBend.Points.Count == 0) continue;
                string pitchSource = GetPitchSource(note, x0, y0);
                DrawPitchPath(pitchSource, x0, y0, i);
                foreach (Ellipse ellipse in GetPitchPoints(note.PitchBend.Points, x0, y0, i))
                    PitchPointCanvas.Children.Add(ellipse);
                i++;
            }
        }

        public void DrawPartPitch()
        {
            PitchOff();
            Part.BuildPartPitch();
            int i = -1;
            do
            {
                i++;
            } while (!Part.Notes[i].IsRest);
            double x0 = (double)Part.Notes[i].NoteControl.GetValue(Canvas.LeftProperty);
            double y0 = (double)Part.Notes[i].NoteControl.GetValue(Canvas.TopProperty) + yScale / 2;
            string pitchSource = GetPitchSource(Part, x0, y0);
            DrawPitchPath(pitchSource, x0, y0);
            foreach (Ellipse ellipse in GetPitchPoints(Part.PitchBend.Points, x0, y0))
                PitchPointCanvas.Children.Add(ellipse);
        }

        private void DrawPitchPath(string pitchSource, double x0, double y0, int i = 0)
        {
            var itt = Math.DivRem(i, 2, out int res);
            Path pitchPath = new Path()
            {
                Stroke = res == 0 ? Themes.pitchBrush : Themes.pitchSecondBrush,
                StrokeThickness = 1,
                Data = Geometry.Parse(pitchSource)
            };
            PitchCanvas.Children.Add(pitchPath);
        }

        public Ellipse[] GetPitchPoints(List<PitchPoint> PitchPoints, double x0, double y0, int i = 0)
        {
            double radius = 5;
            double m = 22.6;
            List<Ellipse> ellipses = new List<Ellipse>();
            foreach (PitchPoint point in PitchPoints)
            {
                var itt = Math.DivRem(i, 2, out int res);
                Ellipse ellipse = new Ellipse()
                {
                    Fill = res == 0 ? Themes.pitchBrush : Themes.pitchSecondBrush,
                    Width = radius,
                    Height = radius
                };
                ellipse.SetValue(Canvas.LeftProperty, x0 + point.X * xScale - radius / 2);
                ellipse.SetValue(Canvas.TopProperty, y0 - point.Y / yScale * m - radius / 2);
                ellipses.Add(ellipse);
            }
            return ellipses.ToArray();
        }

        public string GetPitchSource(Part part, double x0, double y0)
        {
            double c = (yScale);
            int[] pitchData = Part.PitchBend.Array;
            //double val = -note.Oto.Preutter < note.PitchBend.Points.First().X ? -note.Oto.Preutter : note.PitchBend.Points.First().X;
            double val = 0;
            double xP = MusicMath.MillisecondToTick(val) * xScale;
            double y1 = pitchData[0] / c;
            double x1 = 0;
            double m = 2.26;
            string f = "f2";
            string pitchSource = $"M {(x0 + xP).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            for (int i = 0; i < pitchData.Length - 1; i++)
            {
                y1 = pitchData[i] / c;
                x1 += Settings.IntervalTick * xScale;
                pitchSource += $"L {(x0 + xP + x1).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            }
            return pitchSource;
        }

        public string GetPitchSource(Note note, double x0, double y0)
        {
            double c = (yScale);
            int[] pitchData = note.PitchBend.Array;
            Pitch.BuildPitchInfo(note, out PitchInfo pitchInfo);
            // double val = pitchInfo.Start;
            double val = -note.Phoneme.Preutter;
            double xP = MusicMath.MillisecondToTick(val) * xScale;
            double y1 = pitchData[0] / c;
            double x1 = 0;
            double m = 2.26;
            string f = "f4";
            string pitchSource = $"M {(x0 + xP).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            for (int i = 0; i < pitchData.Length - 1; i++)
            {
                y1 = pitchData[i] / c;
                x1 += Settings.IntervalTick * xScale;
                pitchSource += $"L {(x0 + xP + x1).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            }
            return pitchSource;
        }

        private NoteThumb MakeNote(int noteNumber, long startTime, int duration, string lyric)
        {
            NoteThumb noteControl = new NoteThumb(this);
            var top = GetNoteYPosition(noteNumber);
            var left = GetNoteXPosition(startTime);
            noteControl.Text = lyric;
            noteControl.Width = (double)duration * xScale;
            noteControl.Height = yScale;
            noteControl.SetValue(Canvas.TopProperty, top);
            noteControl.SetValue(Canvas.LeftProperty, left);
            return noteControl;
        }

        private double GetNoteYPosition(int noteNumber)
        {
            return (double)(octaves * 12 - 1 - noteNumber) * yScale;
        }

        private double GetNoteXPosition(long startTime)
        {
            return (double)startTime * xScale;
        }

        private void CreateBackgroundCanvas()
        {
            for (int note = 0; note < octaves * 12; note++)
            {
                if ((note % 12 == 1) // C#
                 || (note % 12 == 3) // E#
                 || (note % 12 == 6) // F#
                 || (note % 12 == 8) // G#
                 || (note % 12 == 10)) // A#
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = yScale;
                    rect.Width = RootCanvas.Width;
                    rect.Fill = Themes.blackNoteChannelBrush;
                    rect.SetValue(Canvas.TopProperty, GetNoteYPosition(note));
                    NoteBackgroundCanvas.Children.Add(rect);
                }
            }
            for (int note = 0; note < octaves * 12 - 1; note++)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = RootCanvas.Width;
                line.Y1 = GetNoteYPosition(note);
                line.Y2 = GetNoteYPosition(note);
                line.Stroke = Themes.noteSeparatorBrush;
                NoteBackgroundCanvas.Children.Add(line);
            }
        }

        private void DrawGrid()
        {
            double width = lastPosition > minWidth ? lastPosition : minWidth;
            GridCanvas.Children.Clear();
            int beat = 0;
            for (long n = 0; n < width; n += Settings.Resolution)
            {
                Line line = new Line();
                line.X1 = n * xScale;
                line.X2 = n * xScale;
                line.Y1 = 0;
                line.Y2 = octaves * 12 * yScale;
                if (beat % 4 == 0)
                {
                    line.Stroke = Themes.measureSeparatorBrush;
                }
                else
                {
                    line.StrokeDashCap = PenLineCap.Flat;
                    line.Stroke = Themes.beatSeparatorBrush;
                }
                GridCanvas.Children.Add(line);
                beat++;
            }
        }

        private void RootCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Point currentMousePosition = e.GetPosition(RootCanvas);
                Console.WriteLine($"{currentMousePosition.X}, {currentMousePosition.Y}");

                long startTime = Convert.ToInt64((currentMousePosition.X + scrollViewer.HorizontalOffset) / xScale);
                int MinLength = Settings.Resolution / MaxDivider;
                startTime = (long)Math.Round((double)(startTime / MinLength), 0, MidpointRounding.AwayFromZero) * MinLength;
                int noteNumber = (int)(octaves * 12 - 1 - Math.Round((currentMousePosition.Y + scrollViewer.VerticalOffset) / yScale, 0, MidpointRounding.AwayFromZero));

                int duration = (int)(Settings.Resolution);
                string Lyric = "a";

                Note note = new Note();
                note.NoteNum = noteNumber;
                note.Lyric = Lyric;
                note.Length = duration;
                note.AbsoluteTime = startTime;
                note.Part = Part;
                Part.Notes.Add(note);
                if (Part.Singer.IsEnabled)
                {
                    Part.RefreshPhonemes();
                }                
                DrawNotes();
            }
        }

        private void CreatePiano()
        {

            for (int note = 0; note < octaves * 12; note++)
            {
                if ((note % 12 == 1) // C#
                 || (note % 12 == 3) // E#
                 || (note % 12 == 6) // F#
                 || (note % 12 == 8) // G#
                 || (note % 12 == 10)) // A#
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = yScale;
                    rect.Width = Piano.Width;
                    rect.Fill = Themes.pianoBlackNote;
                    rect.SetValue(Canvas.TopProperty, GetNoteYPosition(note));
                    Piano.Children.Add(rect);
                }
                Label label = new Label();
                string noteName = MusicMath.NoteNum2String(note);
                label.Content = noteName;
                label.Foreground = Themes.pianoNoteNames;
                label.SetValue(Canvas.TopProperty, GetNoteYPosition(note) - 6);
                Console.WriteLine(label.Content);
                Piano.Children.Add(label);
                // label.SetValue(Canvas.LeftProperty, 12);
            }
            for (int note = 0; note < octaves * 12 - 1; note++)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = Piano.Width;
                line.Y1 = GetNoteYPosition(note);
                line.Y2 = GetNoteYPosition(note);
                line.Stroke = Themes.pianoBlackNote;
                Piano.Children.Add(line);
            }
        }

    }
}
