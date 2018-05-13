using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.Midi;
using PianoRoll.Control;
using PianoRoll.Model;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для PianoRollControl.xaml
    /// </summary>
    public partial class PianoRollControl : UserControl
    {

        MidiEventCollection midiEvents;
        private List<UNote> uNotes;
        double xScale = 1.0 / 10;
        double yScale = 15;
        private long lastPosition = 9598;
        private int DeltaTicksPerQuarterNote = 480;

        SolidColorBrush blackNoteChannelBrush = new SolidColorBrush(System.Windows.Media.Colors.LightCyan);
        SolidColorBrush noteSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGray);

        SolidColorBrush measureSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.Black);
        SolidColorBrush beatSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGray);

        public PianoRollControl()
        {
            InitializeComponent();
            DrawInit();
        }

        public MidiEventCollection MidiEvents
        {
            get
            {
                return midiEvents;
            }
            set
            {
                // a quarter note is 20 units wide
                DeltaTicksPerQuarterNote = value.DeltaTicksPerQuarterNote;
                midiEvents = value;
                DrawMidi();
                Resize();
                CreateBackgroundCanvas();
                DrawGrid();
            }
        }

        public List<UNote> UNotes
        {
            get
            {
                return uNotes;
            }
            set
            {
                DeltaTicksPerQuarterNote = 480;
                // a quarter note is 20 units wide
                uNotes = value;
                DrawUst();
                Resize();
                CreateBackgroundCanvas();
                DrawGrid();
            }
        }

        public void Resize()
        {
            xScale = (80.0 / DeltaTicksPerQuarterNote);
            this.Width = lastPosition * xScale;
            this.Height = 128 * yScale;
        }

        public void DrawInit()
        {
            Resize();
            DrawGrid();
            CreateBackgroundCanvas();
        }


        public void DrawMidi()
        {
            NoteCanvas.Children.Clear();
            lastPosition = 0;

            for (int track = 0; track < midiEvents.Tracks; track++)
            {
                foreach (MidiEvent midiEvent in midiEvents[track])
                {
                    if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                    {
                        NoteOnEvent noteOn = (NoteOnEvent)midiEvent;
                        if (noteOn.OffEvent != null)
                        {
                            Rectangle rectangle = MakeNoteRectangle(noteOn.NoteNumber, noteOn.AbsoluteTime, noteOn.NoteLength, noteOn.Channel, out Label label);
                            lastPosition = Math.Max(lastPosition, noteOn.AbsoluteTime + noteOn.NoteLength);
                            NoteCanvas.Children.Add(rectangle);
                            NoteCanvas.Children.Add(label);
                        }
                    }
                }
            }
        }

        public void DrawUst()
        {
            NoteCanvas.Children.Clear();
            lastPosition = 0;

            foreach (UNote note in uNotes)
            {
                NoteControl noteControl = MakeNote(note.NoteNum, note.AbsoluteTime, note.Length, note.Lyric);
                lastPosition = Math.Max(lastPosition, lastPosition + note.Length);
                if (noteControl.Text != "")
                {
                    noteControl.note = note;
                    noteControl.ChangedLyric += DrawUst;
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
                    NoteCanvas.Children.Add(noteControl);
                }
                

            }
            scrollViewer.ScrollToVerticalOffset(540);
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

        private Rectangle MakeNoteRectangle(int noteNumber, long startTime, int duration, int channel, out Label label)
        {
            Rectangle rect = new Rectangle();
            if (channel == 10)
            {
                rect.Stroke = new SolidColorBrush(Colors.DarkGreen);
                rect.Fill = new SolidColorBrush(Colors.LightGreen);
                duration = midiEvents.DeltaTicksPerQuarterNote / 4;
            }
            else
            {
                rect.Stroke = new SolidColorBrush(Colors.DarkBlue);
                rect.Fill = new SolidColorBrush(Colors.LightBlue);
            }
            rect.Width = (double)duration * xScale;
            rect.Height = yScale;
            var top = GetNoteYPosition(noteNumber);
            var left = GetNoteXPosition(startTime);
            rect.SetValue(Canvas.TopProperty, top);
            rect.SetValue(Canvas.LeftProperty, left);
            label = new Label();
            label.Content = noteNumber.ToString();
            label.Margin = new Thickness(0, 0, 0, 0);
            label.SetValue(Canvas.TopProperty, top);
            label.SetValue(Canvas.LeftProperty, left);
            return rect;
        }

        private double GetNoteYPosition(int noteNumber)
        {
            return (double)(127 - noteNumber) * yScale;
        }

        private double GetNoteXPosition(long startTime)
        {
            return (double)startTime * xScale;
        }


        private void CreateBackgroundCanvas()
        {
            for (int note = 0; note < 127; note++)
            {
                if ((note % 12 == 1) // C#
                 || (note % 12 == 3) // E#
                 || (note % 12 == 6) // F#
                 || (note % 12 == 8) // G#
                 || (note % 12 == 10)) // A#
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = yScale;
                    rect.Width = this.Width;
                    rect.Fill = blackNoteChannelBrush;
                    rect.SetValue(Canvas.TopProperty, GetNoteYPosition(note));
                    NoteBackgroundCanvas.Children.Add(rect);
                }
            }
            for (int note = 0; note < 128; note++)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = this.Width;
                line.Y1 = GetNoteYPosition(note);
                line.Y2 = GetNoteYPosition(note);
                line.Stroke = noteSeparatorBrush;
                NoteBackgroundCanvas.Children.Add(line);
            }
        }

        private void DrawGrid()
        {
            GridCanvas.Children.Clear();
            int beat = 0;
            for (long n = 0; n < lastPosition; n += DeltaTicksPerQuarterNote)
            {
                Line line = new Line();
                line.X1 = n * xScale;
                line.X2 = n * xScale;
                line.Y1 = 0;
                line.Y2 = 128 * yScale;
                if (beat % 4 == 0)
                {
                    line.Stroke = measureSeparatorBrush;
                }
                else
                {
                    line.Stroke = beatSeparatorBrush;
                }
                GridCanvas.Children.Add(line);
                beat++;
            }
        }

        private void RootCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(RootCanvas);

            int noteNumber;
            long startTime;
            int duration;
            string Lyric;

            noteNumber = (int)currentMousePosition.Y;
            


        }
    }
}
