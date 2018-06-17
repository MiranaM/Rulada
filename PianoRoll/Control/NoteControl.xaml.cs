using PianoRoll.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public delegate void RedrawUst();

        public event RedrawUst onUstChanged;

        public string Text { get; set; }

        double minwidth;
        double maxwidth;
        double minheight;
        double WidthInit;
        double BorderWidth = 4;
        public DragMode dragMode;
        public PartEditor PartEditor;

        public enum DragMode
        { ResizeLeft, ResizeRight, Move, Mutual, None }



        //public ref UNoteRef;

        public NoteControl(PartEditor partEditor)
        {
            InitializeComponent();

            minwidth = Settings.Resolution / Project.BeatUnit * PartEditor.xScale;
            maxwidth = Settings.Resolution * Project.BeatPerBar * 2 * PartEditor.xScale; // 2 такта
            minheight = PartEditor.yScale;


        }

        public Note note;


        //зафиксить текст _l
        public void SetText (string _l)
        {
            Text = _l;
            this.Lyric.Content = Text;
            this.EditLyric.Text = Text;
            note.Lyric = Text;          
        }

        private void DrawUstAgain(object sender)
        {

        }

        private void Lyric_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.EditLyric.Visibility = Visibility.Visible;
        }

        private void EditLyric_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {                
                //SetText(this.EditLyric.Text);
               // this.EditLyric.Visibility = Visibility.Hidden;
                //onUstChanged();
            }
        }

        private void Lyric_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Part part = Project.Current.Tracks[0].Parts[0];
                part.Notes.Remove(note);
                onUstChanged();
            }

        }

        private void ResizeArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }

        private void EditLyric_LostFocus(object sender, RoutedEventArgs e)
        {
            EditLyric.Visibility = Visibility.Hidden;
        }


        private void ThumbResizeLeft_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double deltaHorizontal;
            double width;

            deltaHorizontal = e.HorizontalChange;
            width = WidthInit - deltaHorizontal;
            if (width > maxwidth) width = maxwidth;
            if (width < minwidth) width = minwidth;
            else Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
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
            width = WidthInit + deltaHorizontal;
            if (width > maxwidth) width = maxwidth;
            if (width < minwidth) width = minwidth;
            Width = width;
        }

        private void ThumbResizeRight_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            WidthInit = Width;
            Mouse.OverrideCursor = Cursors.SizeWE;
        }
    }
}
