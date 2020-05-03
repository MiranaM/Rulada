using PianoRoll.Model;
using PianoRoll.Themes;
using PianoRoll.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl
    {
        double minheight;
        double WidthInit;
        double BorderWidth = 4;
        public DragMode dragMode;
        public PartEditor PartEditor;

        public enum DragMode
        { ResizeLeft, ResizeRight, Move, Mutual, None }

        public delegate void NoteChangedEvent();
        public event NoteChangedEvent OnNoteChanged;
        
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
        public void SetText (string lyric)
        {
            this.Lyric.Content = lyric;
            this.EditLyric.Text = lyric;
            Background = Schemes.unknownBrush;
            ToolTip = "can't find source file";
        }

        public void SetText(string lyric, string phoneme)
        {
            this.Lyric.Content = $"{lyric} [{phoneme}]";
            this.EditLyric.Text = lyric;
            Background = Schemes.noteBrush;
        }
        
        void ComfirmLyric()
        {
            EditLyric.Visibility = Visibility.Hidden;
            note.NewLyric(EditLyric.Text);
            OnNoteChanged();
        }

        private void EditLyric_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) ComfirmLyric();
        }
        
        private void EditLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            ComfirmLyric();
        }


        private void ThumbResizeLeft_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Width -= e.HorizontalChange;
        }

        private void ThumbResizeLeft_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            note.Length = Width / PartEditor.xScale;
            Mouse.OverrideCursor = Cursors.Arrow;
            OnNoteChanged();
        }

        private void ThumbResizeLeft_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            WidthInit = Width;
            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        private void ThumbResizeRight_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            note.Length = Width / PartEditor.xScale;
            Mouse.OverrideCursor = Cursors.Arrow;
            OnNoteChanged();
        }

        private void ThumbResizeRight_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Width += e.HorizontalChange;
        }

        private void ThumbResizeRight_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            WidthInit = Width;
            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        private void ThumbMove_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double deltaHorizontal = Math.Min(e.HorizontalChange, ActualWidth - MinWidth);
            double deltaVertical = Math.Min(e.VerticalChange, ActualWidth - MinWidth);
            Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
            Canvas.SetTop(this, Canvas.GetTop(this) + deltaVertical);
        }

        private void ThumbMove_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            double left = Canvas.GetLeft(this);
            Canvas.SetLeft(this, left);

            double top = Canvas.GetTop(this);
            top += minheight * 0.5;
            top -= (top) % minheight;
            Canvas.SetTop(this, top);
            dragMode = DragMode.None;

            OnNoteChanged();
        }

        void AddNote(Point currentMousePosition)
        {
            double x = Canvas.GetLeft(this) + currentMousePosition.X;
            double y = Canvas.GetTop(this) + currentMousePosition.Y;
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
                Part part = Project.Current.Tracks[0].Parts[0];
                part.Notes.Remove(note);
                PartEditor.Remove(this);
                // OnNoteChanged();
            }
            else EditLyric.Visibility = Visibility.Visible;
        }
    }
}
