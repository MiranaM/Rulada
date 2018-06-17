using PianoRoll.Model;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;

namespace PianoRoll.Control
{
    public class NoteThumb : Thumb
    {
        //public string Lyric = "a";
        public string Text = "a";
        double minwidth;
        double minheight;
        double Speed;
        public DragMode dragMode;

        public enum DragMode
        { ResizeLeft, ResizeRight, Move, None }


        public Note note;

        public NoteThumb()
        {
            this.DragDelta += new DragDeltaEventHandler(this.NoteThumb_DragDelta);
            DragCompleted += new DragCompletedEventHandler(DragCompleted_Thumb);
            MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow;
            MouseEnter += new MouseEventHandler(MouseMove_Thumb);

            minwidth = Settings.Resolution / Project.BeatUnit;
            minheight = PartEditor.yScale;
        }



        private void MouseMove_Thumb(object sender, MouseEventArgs e)
        {

            Point point = e.MouseDevice.GetPosition(this);
            point.X -= Canvas.GetLeft(this);

            double BorderWidth = 10;
            

            if ( point.X < BorderWidth )
            {
                dragMode = DragMode.ResizeLeft;
                Mouse.OverrideCursor = Cursors.SizeWE;

            }
            else if ( point.X > Width-BorderWidth )
            {
                dragMode = DragMode.ResizeRight;
                Mouse.OverrideCursor = Cursors.SizeWE;
            }
            else
            {
                dragMode = DragMode.Move;
                Mouse.OverrideCursor = Cursors.Hand;
            }
        }

        private void DragCompleted_Thumb(object sender, DragCompletedEventArgs e)
        {
            if (dragMode == DragMode.Move)
            {
                double left = Canvas.GetLeft(this);
                left -= left % minwidth;
                Canvas.SetLeft(this, left);

                double top = Canvas.GetTop(this);
                top -= top % minheight;
                Canvas.SetTop(this, top);
                dragMode = DragMode.None;
            }
        }

        public void SetText(string _l)
        {
            Text = _l;
            note.Lyric = Text;
        }

        private void NoteThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double deltaHorizontal;

            if (dragMode == DragMode.Move)
            {            
                deltaHorizontal = Math.Min(e.HorizontalChange, ActualWidth - MinWidth);
                double deltaVertical = Math.Min(e.VerticalChange, ActualWidth - MinWidth);
                Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
                Canvas.SetTop(this, Canvas.GetTop(this) + deltaVertical);
            }
            else if(dragMode == DragMode.ResizeLeft)
            {
                deltaHorizontal = Math.Min(e.HorizontalChange, ActualWidth - MinWidth);
                Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
                
                Width -= deltaHorizontal;
            }
            

            e.Handled = true;
        }
    }
}
