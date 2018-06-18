using PianoRoll.Model;
using PianoRoll.Themes;
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
        double minwidth;
        double maxwidth;
        double minheight;
        double WidthInit;
        double BorderWidth = 4;
        public DragMode dragMode;
        public PartEditor PartEditor;

        public enum DragMode
        { ResizeLeft, ResizeRight, Move, Mutual, None }

        public delegate void RedrawNote();
        public event RedrawNote OnNoteChanged;


        //public ref UNoteRef;

        public NoteControl(PartEditor partEditor)
        {
            PartEditor = partEditor;
            InitializeComponent();

            minwidth = Settings.Resolution / Project.BeatUnit * PartEditor.xScale;
            maxwidth = Settings.Resolution * Project.BeatPerBar * 2 * PartEditor.xScale; // 2 такта
            minheight = PartEditor.yScale;
        }

        public Note note;

        //зафиксить текст lyric, вызывается из Note
        // чтобы отправить на изменение, нужно Note.NewLyric(lyric),
        // чтобы также обработать текст
        public void SetText (string lyric)
        {
            this.Lyric.Content = lyric;
            this.EditLyric.Text = lyric;
            note.Lyric = lyric;
            Background = Schemes.unknownBrush;
            ToolTip = "can't found source file";
        }

        public void SetText(string lyric, Phoneme phoneme)
        {
            this.Lyric.Content = $"{lyric} [{phoneme.Alias}]";
            this.EditLyric.Text = lyric;
            note.Lyric = lyric;
            ToolTip = phoneme.File;
            Background = Schemes.noteBrush;
        }

        private void Lyric_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.EditLyric.Visibility = Visibility.Visible;
        }

        void ComfirmLyric()
        {
            EditLyric.Visibility = Visibility.Hidden;
            note.NewLyric(EditLyric.Text);
        }

        private void EditLyric_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ComfirmLyric();
            }
        }

        private void Lyric_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) 
            {
                Part part = Project.Current.Tracks[0].Parts[0];
                part.Notes.Remove(note);
                PartEditor.Remove(this);
                // OnNoteChanged();
            }
        }
        
        private void EditLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            ComfirmLyric();
        }


        private void ThumbResizeLeft_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double deltaHorizontal;
            double width;

            deltaHorizontal = e.HorizontalChange;
            width = Width - deltaHorizontal;
            if (width > maxwidth) width = maxwidth;
            if (width < minwidth) width = minwidth;

            Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
            Width = width;         
            
        }

        private void ThumbResizeLeft_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            note.Length = Width / PartEditor.xScale;
            Mouse.OverrideCursor = Cursors.Arrow;
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
        }

        private void ThumbResizeRight_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double deltaHorizontal;
            double width;
            
            deltaHorizontal = e.HorizontalChange;
            width = Width + deltaHorizontal;
            if (width > maxwidth) width = maxwidth;
            if (width < minwidth) width = minwidth;
            Width = width;
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
            left += minwidth * 0.5;
            left -= left % minwidth;
            Canvas.SetLeft(this, left);

            double top = Canvas.GetTop(this);
            top += minheight * 0.5;
            top -= (top) % minheight;
            Canvas.SetTop(this, top);
            dragMode = DragMode.None;
        }

        private void ThumbMove_DragStarted(object sender, DragStartedEventArgs e)
        {
            WidthInit = Width;
        }
    }
}
