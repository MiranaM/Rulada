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
using PianoRoll.Themes;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для PartEditor.xaml
    /// </summary>
    public partial class PartEditor : UserControl
    {

        #region variables
        public static double xScale = 1.0 / 15;
        public static double yScale = 15;
        public static bool UseDict = true;
        public static bool UseTrans = true;
        public static bool MustSnap = true;

        public Part Part;
        private long lastPosition;
        public int MaxDivider = 4;
        private int minBars = 4;
        public bool doSnap = true;
        private double minWidth;
        public PitchBendExpression PitchBend;

        public delegate void PartChangedEvent();
        public event PartChangedEvent OnPartChanged;

        public PositionMarker PositionMarker;
        public static PartEditor Instance;
        public static Point ScrollPosition;
        #endregion

        public PartEditor()
        {
            Instance = this;
            OnPartChanged += OnPartChanged_Part;
            xScale = (80.0 / Settings.Resolution);
            minWidth = minBars * Project.BeatPerBar * Settings.Resolution;
            InitializeComponent();
            DrawInit();
            Loaded += OnLoaded_Part;
            Render.OnRenderComplited += OnRenderComplited_PartEditor;
        }

        void OnRenderComplited_PartEditor()
        {
            PositionMarker.MoveAsync();
        }

        public void OnLoaded_Part(object sender, RoutedEventArgs e)
        {
            PositionMarkerCanvas.Width = scrollViewer.ActualWidth;
            PositionMarkerCanvas.Height = scrollViewer.ActualHeight;
            PositionMarker = new PositionMarker(PositionMarkerCanvas, scrollViewer);
        }

        public void OnPartChanged_Part()
        {
            Part.Recalculate();
            Draw();
            PositionMarker = new PositionMarker(PositionMarkerCanvas, scrollViewer);
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
            RootCanvas.Height = Settings.Octaves * 12 * yScale;
        }

        public void Clear()
        {
            GridCanvas.Children.Clear();
            NoteCanvas.Children.Clear();
            NoteBackgroundCanvas.Children.Clear();
            PitchCanvas.Children.Clear();
            PitchPointCanvas.Children.Clear();
            PositionMarkerCanvas.Children.Clear();
            DrawInit();
        }

        public void DrawInit()
        {
            lastPosition = Settings.Resolution * Project.BeatPerBar * minBars;
            Resize();
            DrawGrid();
            CreatePiano();
            CreateBackgroundCanvas();
            lastPosition = 0;
        }

        public void DrawNotes()
        {
            if (Part == null) return;
            Clear();

            foreach (Note note in Part.Notes)
            {
                NoteControl noteControl = MakeNote(note.NoteNum, note.AbsoluteTime, note.Length);
                lastPosition = Math.Max(lastPosition, lastPosition + note.Length);
                note.NoteControl = noteControl;
                NoteCanvas.Children.Add(noteControl);
            }
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
                if (!note.HasPhoneme) continue;
                double x0 = (double)note.NoteControl.GetValue(Canvas.LeftProperty);
                double y0 = (double)note.NoteControl.GetValue(Canvas.TopProperty) + yScale / 2;
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
            int i = 0;
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
                Stroke = res == 0 ? Schemes.pitchBrush : Schemes.pitchSecondBrush,
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
                    Fill = res == 0 ? Schemes.pitchBrush : Schemes.pitchSecondBrush,
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

        private NoteControl MakeNote(int noteNumber, long startTime, int duration)
        {
            NoteControl noteControl = new NoteControl(this);
            var top = MusicMath.GetNoteYPosition(noteNumber);
            var left = MusicMath.GetNoteXPosition(startTime);
            noteControl.Width = (double)duration * xScale;
            noteControl.Height = yScale;
            noteControl.SetValue(Canvas.TopProperty, top);
            noteControl.SetValue(Canvas.LeftProperty, left);
            return noteControl;
        }

        private void CreateBackgroundCanvas()
        {
            for (int note = 0; note < Settings.Octaves * 12; note++)
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
                    rect.Fill = Schemes.blackNoteChannelBrush;
                    rect.SetValue(Canvas.TopProperty, MusicMath.GetNoteYPosition(note));
                    NoteBackgroundCanvas.Children.Add(rect);
                }
            }
            for (int note = 0; note < Settings.Octaves * 12 - 1; note++)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = RootCanvas.Width;
                line.Y1 = MusicMath.GetNoteYPosition(note);
                line.Y2 = MusicMath.GetNoteYPosition(note);
                line.Stroke = Schemes.noteSeparatorBrush;
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
                line.Y2 = Settings.Octaves * 12 * yScale;
                if (beat % 4 == 0)
                {
                    line.Stroke = Schemes.measureSeparatorBrush;
                }
                else
                {
                    line.StrokeDashCap = PenLineCap.Flat;
                    line.Stroke = Schemes.beatSeparatorBrush;
                }
                GridCanvas.Children.Add(line);
                beat++;
            }
            DrawUnitGrid();
        }

        private void DrawUnitGrid()
        {
            double width = lastPosition > minWidth ? lastPosition : minWidth;
            for (long n = 0; n < width; n += Project.MinNoteLengthTick )
            {
                if (n % Settings.Resolution != 0 )
                {
                    Polyline line = new Polyline();
                    line.StrokeDashArray.Add(yScale / 3);
                    line.StrokeDashArray.Add(yScale / 3);
                    line.StrokeDashCap = PenLineCap.Triangle;
                    line.StrokeEndLineCap = PenLineCap.Triangle;
                    line.StrokeStartLineCap = PenLineCap.Triangle;
                    line.Points.Add(new Point(n * xScale, 0));
                    line.Points.Add(new Point(n * xScale, Settings.Octaves * 12 * yScale));
                    line.Stroke = Schemes.beatSeparatorBrush;
                    GridCanvas.Children.Add(line);
                }
            }
        } 

        public void AddNote(double x, double y)
        {
            long startTime = MusicMath.GetAbsoluteTime(x);
            int noteNum = MusicMath.GetNoteNum(y);
            Part.AddNote(startTime, noteNum);
            OnPartChanged();
        }

        public void SetPositionMarker(long sample)
        {
            PositionMarker.MoveTo(sample);
        }

        public void SetPositionMarker(double x)
        {
            PositionMarker.MoveTo(x);
        }

        private void CreatePiano()
        {

            for (int note = 0; note < Settings.Octaves * 12; note++)
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
                    rect.Fill = Schemes.pianoBlackNote;
                    rect.SetValue(Canvas.TopProperty, MusicMath.GetNoteYPosition(note));
                    Piano.Children.Add(rect);
                }
                Label label = new Label();
                string noteName = MusicMath.NoteNum2String(note);
                label.Content = noteName;
                label.Foreground = Schemes.pianoNoteNames;
                label.SetValue(Canvas.TopProperty, MusicMath.GetNoteYPosition(note) - 6);
                Console.WriteLine(label.Content);
                Piano.Children.Add(label);
                // label.SetValue(Canvas.LeftProperty, 12);
            }
            for (int note = 0; note < Settings.Octaves * 12 - 1; note++)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = Piano.Width;
                line.Y1 = MusicMath.GetNoteYPosition(note);
                line.Y2 = MusicMath.GetNoteYPosition(note);
                line.Stroke = Schemes.pianoBlackNote;
                Piano.Children.Add(line);
            }
        }


        public void Remove(NoteControl note)
        {
            NoteCanvas.Children.Remove(note);
        }

        private void UseTransCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UseTrans = UseTransCheckBox.IsChecked.Value;
            OnPartChanged();
        }

        private void UseDictCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UseDict = UseDictCheckBox.IsChecked.Value;
            OnPartChanged();
        }

        private void SnapCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MustSnap = SnapCheckBox.IsChecked.Value;
            OnPartChanged();
        }

        private void RootCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(RootCanvas);
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                AddNote(position.X, position.Y);
            }
            else
            {
                SetPositionMarker(position.X);
            }
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollPosition = new Point(e.HorizontalOffset, e.VerticalOffset);
        }
    }
}
