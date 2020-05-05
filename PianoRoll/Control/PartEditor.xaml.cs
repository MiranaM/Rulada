using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PianoRoll.Model;
using PianoRoll.Model.Pitch;
using PianoRoll.Themes;
using PianoRoll.Util;
using static PianoRoll.Settings;

namespace PianoRoll.Control
{
    /// <summary>
    ///     Логика взаимодействия для PartEditor.xaml
    /// </summary>
    public partial class PartEditor : UserControl
    {
        #region variables
        public static bool UseDict = true;
        public static bool UseTrans = true;
        public static bool MustSnap = true;

        public Part Part;
        private long lastPosition;
        public int MaxDivider = 4;
        private readonly int minBars = 16;
        public bool doSnap = true;
        private readonly double minWidth;
        public PitchBendExpression PitchBend;

        public delegate void PartChangedEvent();

        public event PartChangedEvent OnPartChanged;

        public PositionMarker PositionMarker;
        public static PartEditor Instance;
        public static Point ScrollPosition;

        public List<NoteControl> NoteControls = new List<NoteControl>();

        #endregion

        public PartEditor()
        {
            Instance = this;
            OnPartChanged += OnPartChanged_Part;
            //xScale = 480.0 / Settings.RESOLUTION;
            minWidth = minBars *
                       Settings.Current.BeatPerBar 
                       * RESOLUTION;
            InitializeComponent();
            DrawInit();
            Loaded += OnLoaded_Part;
            Render.Current.OnRenderComplited += OnRenderComplited_PartEditor;
        }

        private void OnRenderComplited_PartEditor()
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
            DrawNotes();
            //PositionMarker = new PositionMarker(PositionMarkerCanvas, scrollViewer);
        }

        public void DrawPart()
        {
            Resize();
            CreateBackgroundCanvas();
            DrawGrid();
        }

        public void Resize()
        {
            RootCanvas.Width = lastPosition > minWidth ? lastPosition * Settings.Current.xScale : minWidth * Settings.Current.xScale;
            RootCanvas.Height = Settings.Current.Octaves * 12 * Settings.Current.yScale;
        }

        public void Clear()
        {
            while (NoteControls.Count > 0)
                DeleteNoteControl(NoteControls[0]);
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
            lastPosition = RESOLUTION * Settings.Current.BeatPerBar * minBars;
            DrawPart();
            CreatePiano();
            lastPosition = 0;
        }

        public void DrawNotes()
        {
            // BUG: crash when open another project
            if (NoteCanvas.Children.Count != NoteControls.Count)
                throw new Exception();

            if (Part == null) return;

            var i = 0;
            foreach (var note in Part.Notes)
            {
                NoteControl noteControl;
                if (i < NoteControls.Count)
                    noteControl = NoteControls[i];
                else
                {
                    noteControl = new NoteControl(this);
                    NoteCanvas.Children.Add(noteControl);
                    NoteControls.Add(noteControl);
                    noteControl.OnNoteDeleted += DeleteNoteControl;
                }
                MakeNote(noteControl, note.NoteNum, note.FinalPosition, note.FinalLength);
                lastPosition = Math.Max(lastPosition, lastPosition + note.FinalLength);
                note.SetNoteControl(noteControl);

                i++;
            }

            while (i < NoteControls.Count)
            {
                NoteControls[i].note = null; // now may refer to a note from different noteControl
                DeleteNoteControl(NoteControls[i]);
            }
        }

        public void DeleteNoteControl(NoteControl noteControl)
        {
            NoteCanvas.Children.Remove(noteControl);
            NoteControls.Remove(noteControl);
            if (noteControl.note != null)
            {
                Part.DeleteNote(noteControl.note);
            }
        }

        public void PitchOff()
        {
            PitchCanvas.Children.Clear();
            PitchPointCanvas.Children.Clear();
        }

        private void MakeNote(NoteControl noteControl, int noteNumber, long startTime, int duration)
        {
            var top = MusicMath.Current.GetNoteYPosition(noteNumber);
            var left = MusicMath.Current.GetNoteXPosition(startTime);
            noteControl.Width = duration * Settings.Current.xScale;
            noteControl.grid.RowDefinitions[1].Height = new GridLength(Settings.Current.yScale);
            noteControl.SetValue(Canvas.TopProperty, top);
            noteControl.SetValue(Canvas.LeftProperty, left);
        }

        private void CreateBackgroundCanvas()
        {
            for (var note = 0; note < Settings.Current.Octaves * 12; note++)
                if (note % 12 == 1 // C#
                    || note % 12 == 3 // E#
                    || note % 12 == 6 // F#
                    || note % 12 == 8 // G#
                    || note % 12 == 10) // A#
                {
                    var rect = new Rectangle();
                    rect.Height = Settings.Current.yScale;
                    rect.Width = RootCanvas.Width;
                    rect.Fill = Schemes.Current.blackNoteChannelBrush;
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
                line.Stroke = Schemes.Current.noteSeparatorBrush;
                NoteBackgroundCanvas.Children.Add(line);
            }
        }

        private void DrawGrid()
        {
            var width = lastPosition > minWidth ? lastPosition : minWidth;
            GridCanvas.Children.Clear();
            var beat = 0;
            for (long n = 0; n < width; n += RESOLUTION)
            {
                var line = new Line();
                line.X1 = n * Settings.Current.xScale;
                line.X2 = n * Settings.Current.xScale;
                line.Y1 = 0;
                line.Y2 = Settings.Current.Octaves * 12 * Settings.Current.yScale;
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
                if (n % Settings.RESOLUTION != 0)
                {
                    var line = new Polyline();
                    line.StrokeDashArray.Add(Settings.Current.yScale / 3);
                    line.StrokeDashArray.Add(Settings.Current.yScale / 3);
                    line.StrokeDashCap = PenLineCap.Triangle;
                    line.StrokeEndLineCap = PenLineCap.Triangle;
                    line.StrokeStartLineCap = PenLineCap.Triangle;
                    line.Points.Add(new Point(n * Settings.Current.xScale, 0));
                    line.Points.Add(new Point(n * Settings.Current.xScale, Settings.Current.Octaves * 12 * Settings.Current.yScale));
                    line.Stroke = Schemes.Current.beatSeparatorBrush;
                    GridCanvas.Children.Add(line);
                }
        }

        public void AddNote(double x, double y)
        {
            var startTime = MusicMath.Current.GetAbsoluteTime(x);
            var noteNum = MusicMath.Current.GetNoteNum(y);
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
            // TODO: scroll with view
            for (var note = 0; note < Settings.Current.Octaves * 12; note++)
            {
                if (note % 12 == 1 // C#
                    || note % 12 == 3 // E#
                    || note % 12 == 6 // F#
                    || note % 12 == 8 // G#
                    || note % 12 == 10) // A#
                {
                    var rect = new Rectangle();
                    rect.Height = Settings.Current.yScale;
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

        private void RootCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(RootCanvas);
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                AddNote(position.X, position.Y);
            else
                SetPositionMarker(position.X);
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollPosition = new Point(e.HorizontalOffset, e.VerticalOffset);
        }

        private void ZoomUpButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Current.xScale = Settings.Current.xScale * 2;
            DrawNotes();
            DrawPart();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Current.xScale = Settings.Current.xScale / 2;
            DrawNotes();
            DrawPart();
        }
    }
}