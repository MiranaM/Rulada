﻿using System;
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

        private List<UNote> uNotes;
        double xScale = 1.0 / 10;
        double yScale = 15;
        private long lastPosition = 9598;
        public int MaxDivider = 4;
        public bool doSnap = true;
        public int octaves = 7;

        SolidColorBrush blackNoteChannelBrush = new SolidColorBrush(System.Windows.Media.Colors.LightCyan);
        SolidColorBrush noteSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGray);

        SolidColorBrush measureSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.Black);
        SolidColorBrush beatSeparatorBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGray);

        public PianoRollControl()
        {
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
            xScale = (80.0 / Settings.Resolution);
            PianoRollGrid.Width = lastPosition * xScale;
            PianoRollGrid.Height = octaves * 12 * yScale;
            RootCanvas.Width = PianoRollGrid.Width;
            RootCanvas.Height = PianoRollGrid.Height;
        }

        public void DrawInit()
        {
            Resize();
            DrawGrid();
            CreatePiano();
            CreateBackgroundCanvas();
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
                    NoteCanvas.Children.Add(noteControl);
                }
                

            }
            //scrollViewer.ScrollToVerticalOffset(540);
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
                    rect.Width = PianoRollGrid.Width;
                    rect.Fill = blackNoteChannelBrush;
                    rect.SetValue(Canvas.TopProperty, GetNoteYPosition(note));
                    NoteBackgroundCanvas.Children.Add(rect);
                }
            }
            for (int note = 0; note < octaves * 12 - 1; note++)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = PianoRollGrid.Width;
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
            for (long n = 0; n < lastPosition; n += Settings.Resolution)
            {
                Line line = new Line();
                line.X1 = n * xScale;
                line.X2 = n * xScale;
                line.Y1 = 0;
                line.Y2 = octaves * 12 * yScale;
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
                    rect.Fill = blackNoteChannelBrush;
                    rect.SetValue(Canvas.TopProperty, GetNoteYPosition(note));
                    Piano.Children.Add(rect);
                }
                Label label = new Label();
                string noteName = Ust.NoteNum2String(note);
                label.Content = noteName;
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
                line.Stroke = noteSeparatorBrush;
                Piano.Children.Add(line);
            }
        }

    }
}
