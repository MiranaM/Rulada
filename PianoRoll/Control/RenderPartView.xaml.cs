using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PianoRoll.Model;
using PianoRoll.Themes;
using PianoRoll.Util;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для RenderPartView.xaml
    /// </summary>
    public partial class RenderPartView : UserControl
    {
        public double xScale = 480 / Settings.Current.Resolution;
        public double yScale = 15;

        public Part Part;
        private long lastPosition;
        private readonly int minBars = 16;
        private readonly double minWidth;

        public List<RenderNoteView> NoteControls = new List<RenderNoteView>();

        public RenderPartView()
        {
            minWidth = minBars * Settings.Current.BeatPerBar * Settings.Current.Resolution;
            InitializeComponent();
            CreatePiano();
        }

        public void SetPart(Part part)
        {
            Part = part;
            lastPosition = Settings.Current.Resolution * Settings.Current.BeatPerBar * minBars;
            DrawPart();
            lastPosition = 0;
        }

        public void DrawPart()
        {
            Resize();
            CreateBackgroundCanvas();
            DrawGrid();
            DrawNotes();
            //DrawPartPitch();
        }

        public void Resize()
        {
            RootCanvas.Width = lastPosition > minWidth ? lastPosition * xScale : minWidth * xScale;
            RootCanvas.Height = Settings.Current.Octaves * 12 * yScale;
        }

        public void DrawPitch()
        {
            PitchOff();
            Part.BuildPitch();
            var i = 0;
            foreach (var note in Part.Notes)
            {
                if (!note.HasPhoneme) continue;
                var x0 = (double)note.NoteControl.GetValue(Canvas.LeftProperty);
                var y0 = (double)note.NoteControl.GetValue(Canvas.TopProperty) + yScale / 2;
                var pitchSource = GetPitchSource(note, x0, y0);
                DrawPitchPath(pitchSource, x0, y0, i);
                var points = GetPitchPoints(note.PitchBend.Points, x0, y0, i);
                foreach (var ellipse in points)
                    PitchPointCanvas.Children.Add(ellipse);
                i++;
            }
        }

        public void DrawPartPitch()
        {
            PitchOff();
            Part.BuildPartPitch();
            var i = 0;
            var x0 = (double)Part.Notes[i].NoteControl.GetValue(Canvas.LeftProperty);
            var y0 = (double)Part.Notes[i].NoteControl.GetValue(Canvas.TopProperty) + yScale / 2;
            var pitchSource = GetPitchSource(Part, x0, y0);
            DrawPitchPath(pitchSource, x0, y0);
            foreach (var ellipse in GetPitchPoints(Part.PitchBend.Points, x0, y0))
                PitchPointCanvas.Children.Add(ellipse);
        }

        private void DrawPitchPath(string pitchSource, double x0, double y0, int i = 0)
        {
            var itt = Math.DivRem(i, 2, out var res);
            var pitchPath = new Path
            {
                Stroke = res == 0 ? Schemes.Current.pitchBrush : Schemes.Current.pitchSecondBrush,
                StrokeThickness = 1,
                Data = Geometry.Parse(pitchSource)
            };
            PitchCanvas.Children.Add(pitchPath);
        }

        public Ellipse[] GetPitchPoints(List<PitchPoint> PitchPoints, double x0, double y0, int i = 0)
        {
            double radius = 5;
            var m = 22.6;
            var ellipses = new List<Ellipse>();
            foreach (var point in PitchPoints)
            {
                var itt = Math.DivRem(i, 2, out var res);
                var ellipse = new Ellipse
                {
                    Fill = res == 0 ? Schemes.Current.pitchBrush : Schemes.Current.pitchSecondBrush,
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
            return "";
            var c = yScale;
            var pitchData = Part.PitchBend.Array;
            //double val = -note.Oto.Preutter < note.PitchBend.Points.First().X ? -note.Oto.Preutter : note.PitchBend.Points.First().X;
            double val = 0;
            var xP = MusicMath.Current.MillisecondToTick(val) * xScale;
            var y1 = pitchData[0] / c;
            double x1 = 0;
            var m = 2.26;
            var f = "f2";
            var pitchSource = $"M {(x0 + xP).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            for (var i = 0; i < pitchData.Length - 1; i++)
            {
                y1 = pitchData[i] / c;
                x1 += Settings.Current.IntervalTick * xScale;
                pitchSource += $"L {(x0 + xP + x1).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            }

            return pitchSource;
        }

        public string GetPitchSource(Note note, double x0, double y0)
        {
            var c = yScale;
            var pitchData = note.PitchBend.Array;
            Pitch.Current.BuildPitchInfo(note, out var pitchInfo);
            // double val = pitchInfo.Start;
            var val = -note.Phoneme.Preutter;
            var xP = MusicMath.Current.MillisecondToTick(val) * xScale;
            var y1 = pitchData[0] / c;
            double x1 = 0;
            var m = 2.26;
            var f = "f4";
            var pitchSource = $"M {(x0 + xP).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            for (var i = 0; i < pitchData.Length - 1; i++)
            {
                y1 = pitchData[i] / c;
                x1 += Settings.Current.IntervalTick * xScale;
                pitchSource += $"L {(x0 + xP + x1).ToString(f)} {(y0 - y1 * m).ToString(f)} ";
            }

            return pitchSource;
        }

        private void MakeNote(RenderNoteView noteControl, int noteNumber, long startTime, int duration)
        {
            var top = MusicMath.Current.GetNoteYPosition(noteNumber);
            var left = MusicMath.Current.GetNoteXPosition(startTime);
            noteControl.Width = duration * xScale;
            noteControl.Height = yScale;
            noteControl.SetValue(Canvas.TopProperty, top);
            noteControl.SetValue(Canvas.LeftProperty, left);
        }

        private void CreateBackgroundCanvas()
        {
            var brush1 = new SolidColorBrush(Color.FromRgb(192, 199, 187));
            var brush2 = new SolidColorBrush(Color.FromRgb(232, 237, 228));
            var brush3 = new SolidColorBrush(Color.FromRgb(218, 222, 215));
            for (var note = 0; note < Settings.Current.Octaves * 12; note++)
                if (note % 12 == 1 // C#
                    || note % 12 == 3 // E#
                    || note % 12 == 6 // F#
                    || note % 12 == 8 // G#
                    || note % 12 == 10) // A#
                {
                    var rect = new Rectangle();
                    rect.Height = yScale;
                    rect.Width = RootCanvas.Width;
                    rect.Fill = brush3;
                    rect.SetValue(Canvas.TopProperty, MusicMath.Current.GetNoteYPosition(note));
                    NoteBackgroundCanvas.Children.Add(rect);
                }

            for (var note = 0; note < Settings.Current.Octaves * 12 - 1; note++)
            {
                var line = new Line();
                line.X1 = 0;
                line.X2 = RootCanvas.Width;
                line.Y1 = MusicMath.Current.GetNoteYPosition(note);
                line.Y2 = MusicMath.Current.GetNoteYPosition(note);
                line.Fill = brush2;
                line.Stroke = brush1;
                NoteBackgroundCanvas.Children.Add(line);
            }
        }

        private void DrawGrid()
        {
            var width = lastPosition > minWidth ? lastPosition : minWidth;
            GridCanvas.Children.Clear();
            var beat = 0;
            for (long n = 0; n < width; n += Settings.Current.Resolution)
            {
                var line = new Line();
                line.X1 = n * xScale;
                line.X2 = n * xScale;
                line.Y1 = 0;
                line.Y2 = Settings.Current.Octaves * 12 * yScale;
                if (beat % 4 == 0)
                {
                    line.Stroke = Schemes.Current.measureSeparatorBrush;
                }
                else
                {
                    line.StrokeDashCap = PenLineCap.Flat;
                    line.Stroke = Schemes.Current.beatSeparatorBrush;
                }

                GridCanvas.Children.Add(line);
                beat++;
            }

            DrawUnitGrid();
        }

        private void DrawUnitGrid()
        {
            var width = lastPosition > minWidth ? lastPosition : minWidth;
            for (double n = 0; n < width; n += Settings.Current.MinNoteLengthTick)
                if (n % Settings.Current.Resolution != 0)
                {
                    var line = new Polyline();
                    line.StrokeDashArray.Add(yScale / 3);
                    line.StrokeDashArray.Add(yScale / 3);
                    line.StrokeDashCap = PenLineCap.Triangle;
                    line.StrokeEndLineCap = PenLineCap.Triangle;
                    line.StrokeStartLineCap = PenLineCap.Triangle;
                    line.Points.Add(new Point(n * xScale, 0));
                    line.Points.Add(new Point(n * xScale, Settings.Current.Octaves * 12 * yScale));
                    line.Stroke = Schemes.Current.beatSeparatorBrush;
                    GridCanvas.Children.Add(line);
                }
        }

        public void DrawNotes()
        {
            if (NoteCanvas.Children.Count != NoteControls.Count)
                throw new Exception();

            if (Part == null) return;

            var i = 0;
            foreach (var note in Part.Notes)
            {
                RenderNoteView noteControl;
                if (i < NoteControls.Count)
                    noteControl = NoteControls[i];
                else
                {
                    noteControl = new RenderNoteView(this);
                    NoteCanvas.Children.Add(noteControl);
                    NoteControls.Add(noteControl);
                }
                MakeNote(noteControl, note.NoteNum, note.AbsoluteTime, note.Length);
                noteControl.SetNote(note);
                lastPosition = Math.Max(lastPosition, lastPosition + note.Length);

                i++;
            }
        }

        public void PitchOff()
        {
            PitchCanvas.Children.Clear();
            PitchPointCanvas.Children.Clear();
        }

        private void CreatePiano()
        {
            for (var note = 0; note < Settings.Current.Octaves * 12; note++)
            {
                if (note % 12 == 1 // C#
                    || note % 12 == 3 // E#
                    || note % 12 == 6 // F#
                    || note % 12 == 8 // G#
                    || note % 12 == 10) // A#
                {
                    var rect = new Rectangle();
                    rect.Height = yScale;
                    rect.Width = Piano.Width;
                    rect.Fill = Schemes.Current.pianoBlackNote;
                    rect.SetValue(Canvas.TopProperty, MusicMath.Current.GetNoteYPosition(note));
                    Piano.Children.Add(rect);
                }

                var label = new Label();
                var noteName = MusicMath.Current.NoteNum2String(note);
                label.Content = noteName;
                label.Foreground = Schemes.Current.pianoNoteNames;
                label.SetValue(Canvas.TopProperty, MusicMath.Current.GetNoteYPosition(note) - 6);
                Piano.Children.Add(label);
                // label.SetValue(Canvas.LeftProperty, 12);
            }

            for (var note = 0; note < Settings.Current.Octaves * 12 - 1; note++)
            {
                var line = new Line();
                line.X1 = 0;
                line.X2 = Piano.Width;
                line.Y1 = MusicMath.Current.GetNoteYPosition(note);
                line.Y2 = MusicMath.Current.GetNoteYPosition(note);
                line.Stroke = Schemes.Current.pianoBlackNote;
                Piano.Children.Add(line);
            }
        }

        private void ZoomUpButton_Click(object sender, RoutedEventArgs e)
        {
            xScale = xScale * 2;
            DrawNotes();
            DrawPart();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            xScale = xScale / 2;
            DrawNotes();
            DrawPart();
        }
    }
}
