using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PianoRoll.Model;
using PianoRoll.Themes;
using PianoRoll.Util;

namespace PianoRoll.Control
{
    /// <summary>
    ///     Логика взаимодействия для NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl
    {
        private readonly double minheight;
        private double WidthInit;
        private double BorderWidth = 4;
        public DragMode dragMode;
        public PartEditor PartEditor;

        public enum DragMode
        {
            ResizeLeft,
            ResizeRight,
            Move,
            Mutual,
            None
        }

        public delegate void NoteChangedEvent();

        public event NoteChangedEvent OnNoteChanged = delegate {  };

        public NoteControl(PartEditor partEditor)
        {
            PartEditor = partEditor;
            InitializeComponent();
            minheight = PartEditor.yScale;
            OnNoteChanged += OnNoteChanged_Note;
        }

        public void Delete()
        {
            OnNoteChanged();
        }

        public void OnNoteChanged_Note()
        {
            PartEditor.OnPartChanged_Part();
        }

        public Note note;

        // зафиксить текст lyric, вызывается из Note
        // чтобы отправить на изменение, нужно Note.NewLyric(lyric),
        // чтобы также обработать текст
        public void SetText(string lyric)
        {
            Lyric.Content = lyric;
            EditLyric.Text = lyric;
            ThumbMove.IsEnabled = false;
            ToolTip = "can't find source file";
        }

        public void SetText(string lyric, string phoneme)
        {
            Lyric.Content = lyric;
            Phoneme.Content = $"[{phoneme}]";
            EditLyric.Text = lyric;
            ThumbMove.IsEnabled = true;
        }

        private void ConfirmLyric()
        {
            EditLyric.Visibility = Visibility.Hidden;
            var text = EditLyric.Text;
            if (text.Substring(0, 1) == "/")
            {
                note.Lyric = text;
                note.Phonemes = text.Substring(1);
            }
            else
            {
                var texts = text.Split(' ');
                var noteToSet = note;
                var i = 0;
                while (noteToSet != null && i < texts.Length)
                {
                    noteToSet.NewLyric(texts[i]);
                    i++;
                    noteToSet = noteToSet.GetNext();
                }
            }
            // possible BUG: need OnNoteChanged on all of them?
            OnNoteChanged();
        }

        private void EditLyric_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) ConfirmLyric();
        }

        private void EditLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EditLyric.Visibility == Visibility.Visible)
                ConfirmLyric();
        }

        private void ThumbResizeLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Width -= e.HorizontalChange;
        }

        private void ThumbResizeLeft_DragCompleted(object sender,
            DragCompletedEventArgs e)
        {
            note.Length = Width / PartEditor.xScale;
            OnNoteChanged();
        }

        private void ThumbResizeLeft_DragStarted(object sender,
            DragStartedEventArgs e)
        {
            WidthInit = Width;
        }

        private void ThumbResizeRight_DragCompleted(object sender,
            DragCompletedEventArgs e)
        {
            note.Length = Width / PartEditor.xScale;
            OnNoteChanged();
        }

        private void ThumbResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Width += e.HorizontalChange;
        }

        private void ThumbResizeRight_DragStarted(object sender,
            DragStartedEventArgs e)
        {
            WidthInit = Width;
        }

        private void ThumbMove_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var deltaHorizontal = Math.Min(e.HorizontalChange, ActualWidth - MinWidth);
            var deltaVertical = Math.Min(e.VerticalChange, ActualWidth - MinWidth);
            Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
            Canvas.SetTop(this, Canvas.GetTop(this) + deltaVertical);
        }

        private void ThumbMove_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var left = Canvas.GetLeft(this);
            Canvas.SetLeft(this, left);

            var top = Canvas.GetTop(this);
            top += minheight * 0.5;
            top -= top % minheight;
            Canvas.SetTop(this, top);
            dragMode = DragMode.None;

            OnNoteChanged();
        }

        private void AddNote(Point currentMousePosition)
        {
            var x = Canvas.GetLeft(this) + currentMousePosition.X;
            var y = Canvas.GetTop(this) + currentMousePosition.Y;
            double time = MusicMath.GetNoteXPosition(note.AbsoluteTime);
            note.Length = MusicMath.GetAbsoluteTime(x - time);
            PartEditor.AddNote(x, y);
            // OnNoteChanged is already called in "add note"
        }

        private void ThumbMove_DragStarted(object sender, DragStartedEventArgs e)
        {
            WidthInit = Width;
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) AddNote(Mouse.GetPosition(this));
        }

        private void ThumbMove_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var part = Project.Current.Tracks[0].Parts[0];
                part.Notes.Remove(note);
                PartEditor.Remove(this);
                // OnNoteChanged();
            }
            else
            {
                EditLyric.Visibility = Visibility.Visible;
            }
        }
    }
}