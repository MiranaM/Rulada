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
using PianoRoll.Util;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для PianoRollControl.xaml
    /// </summary>
    public partial class PianoRollControl : UserControl
    {

        #region variables
        private List<UNote> uNotes;
        double xScale = 1.0 / 15;
        double yScale = 15;
        private long lastPosition;
        public int MaxDivider = 4;
        public bool doSnap = true;
        public int octaves = 7;
        private int minBars = 4;
        private double minWidth;
        private bool isPitchShown = false;

        //SolidColorBrush blackNoteChannelBrush = new SolidColorBrush(System.Windows.Media.Colors.LightCyan);
        //SolidColorBrush noteSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGray);

        //SolidColorBrush measureSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.Black);
        //SolidColorBrush beatSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGray);
        //SolidColorBrush pitchBrush = new SolidColorBrush(System.Windows.Media.Colors.LightCoral);

        #endregion

        public PianoRollControl()
        {
            xScale = (80.0 / Settings.Resolution);
            minWidth = minBars * Settings.BeatPerBar * Settings.Resolution;
            InitializeComponent();
            DrawInit();
        }

        public List<UNote> UNotes
        {
            get { return uNotes; }
            set
            {
                uNotes = value;
                DrawUst();
                Resize();
                CreateBackgroundCanvas();
                DrawGrid();
            }
        }

        public void Resize()
        {
            RootCanvas.Width = lastPosition > minWidth ? lastPosition * xScale : minWidth * xScale;
            RootCanvas.Height = octaves * 12 * yScale;
        }

        public void DrawInit()
        {
            lastPosition = Settings.Resolution * Settings.BeatPerBar * minBars;
            Resize();
            DrawGrid();
            CreatePiano();
            CreateBackgroundCanvas();
        }

        public void DrawUst()
        {
            NoteCanvas.Children.Clear();
            PitchCanvas.Children.Clear();
            PitchPointCanvas.Children.Clear();
            lastPosition = 0;
            int i = 0;

            foreach (UNote note in uNotes)
            {
                NoteControl noteControl = MakeNote(note.NoteNum, note.AbsoluteTime, note.Length, note.Lyric);
                lastPosition = Math.Max(lastPosition, lastPosition + note.Length);
                if (!note.IsRest)
                {
                    noteControl.note = note;
                    noteControl.onUstChanged += DrawUst;
                    noteControl.SetText(note.Lyric);
                    if (note.HasOto)
                    {
                        noteControl.ToolTip = note.Oto.File;
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

        public bool DrawPitch()
        {
            if (isPitchShown)
            {
                PitchCanvas.Children.Clear();
                PitchPointCanvas.Children.Clear();
                isPitchShown = false;
                return false;
            }
            else
            {
                if (UNotes == null) return false;
                int i = 0;
                foreach (UNote note in UNotes)
                {
                    if (!note.HasOto || note.IsRest) continue;
                    double x0 = (double)note.NoteControl.GetValue(Canvas.LeftProperty);
                    double y0 = (double)note.NoteControl.GetValue(Canvas.TopProperty) + yScale / 2;
                    DrawPitchPath(note, x0, y0, i);
                    i++;
                }
                isPitchShown = true;
                return true;
            }
        }

        private void DrawPitchPath(UNote note, double x0, double y0, int i = 0)
        {
            if (note.PitchBend.Points.Count == 0) return;
            string pitchSource = PitchDataToPath(note, x0, y0);

            var itt = Math.DivRem(i, 2, out int res);
            Path pitchPath = new Path()
            {
                Stroke = res == 0 ? Themes.pitchBrush : Themes.pitchSecondBrush,
                StrokeThickness = 1,
                Data = Geometry.Parse(pitchSource)
            };

            PitchCanvas.Children.Add(pitchPath);
            foreach (Ellipse ellipse in GetPitchPoints(note, x0, y0, i))
            {
                PitchPointCanvas.Children.Add(ellipse);
            }
        }

        public Ellipse[] GetPitchPoints(UNote note, double x0, double y0, int i = 0)
        {
            double radius = 5;
            double m = 2.26;
            List<Ellipse> ellipses = new List<Ellipse>();
            foreach (PitchPoint point in note.PitchBend.Points)
            {
                var itt = Math.DivRem(i, 2, out int res);
                Ellipse ellipse = new Ellipse()
                {
                    Fill = res == 0 ? Themes.pitchBrush : Themes.pitchSecondBrush,
                    Width = radius,
                    Height = radius
                };
                ellipse.SetValue(Canvas.LeftProperty, x0 + point.X *xScale - radius/2);
                ellipse.SetValue(Canvas.TopProperty, y0 - point.Y / yScale * m - radius / 2);
                ellipses.Add(ellipse);
            }
            return ellipses.ToArray();
        }
        
        public string PitchDataToPath(UNote note, double x0, double y0)
        {
            double c = (yScale);
            int[] pitchData = note.PitchBend.Array;
            //double val = -note.Oto.Preutter < note.PitchBend.Points.First().X ? -note.Oto.Preutter : note.PitchBend.Points.First().X;
            double val = -note.Oto.Preutter;
            double xP = Ust.MillisecondToTick(val) * xScale;
            double y1 = pitchData[0] / c;
            double x1 = 0;
            double m = 2.26;
            string pitchSource = $"M {x0 + xP} {y0 - y1 * m} ";
            for (int i = 0; i < pitchData.Length - 1; i++)
            {
                y1 = pitchData[i] / c;
                x1 += Settings.IntervalTick * xScale;
                pitchSource += $"L {x0 + xP + x1} {y0 - y1 * m} ";
            }
            return pitchSource;
        }

        private NoteControl MakeNote(int noteNumber, long startTime, int duration, string lyric)
        {
            NoteControl noteControl = new NoteControl();
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
                startTime = (long) Math.Round((double)(startTime / MinLength), 0, MidpointRounding.AwayFromZero) * MinLength;
                int noteNumber = (int) (octaves * 12 - 1 - Math.Round((currentMousePosition.Y + scrollViewer.VerticalOffset) / yScale, 0, MidpointRounding.AwayFromZero));

                int duration = (int)(Settings.Resolution);
                string Lyric = "a";

                UNote uNote = new UNote();
                uNote.SetDefaultNoteSettings();
                uNote.NoteNum = noteNumber;
                uNote.Lyric = Lyric;
                uNote.Length = duration;
                uNote.AbsoluteTime = startTime;
                Ust.NotesList.Add(uNote);
                USinger.NoteOtoRefresh();
                DrawUst();
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
                string noteName = Ust.NoteNum2String(note);
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
