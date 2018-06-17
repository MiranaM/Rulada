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
        double maxwidth;
        double minheight;
        double WidthInit;
        double BorderWidth = 4;
        public DragMode dragMode;
        public PartEditor PartEditor;

        public enum DragMode
        { ResizeLeft, ResizeRight, Move, Mutual, None }


        public Note note;

        public NoteThumb(PartEditor partEditor)
        {
            PartEditor = partEditor;
            DragStarted += new DragStartedEventHandler(DragEnter_Thumb);
            DragDelta += new DragDeltaEventHandler(this.NoteThumb_DragDelta);
            DragCompleted += new DragCompletedEventHandler(DragCompleted_Thumb);
            MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow;
            // MouseEnter += new MouseEventHandler(MouseMove_Thumb);
            MouseMove += new MouseEventHandler(MouseMove_Thumb);

            minwidth = Settings.Resolution / Project.BeatUnit * PartEditor.xScale;
            maxwidth = Settings.Resolution * Project.BeatPerBar * 2 * PartEditor.xScale; // 2 такта
            minheight = PartEditor.yScale;
        }

        private void DragEnter_Thumb(object sender, DragStartedEventArgs e)
        {
            WidthInit = Width;
        }

        private void MouseMove_Thumb(object sender, MouseEventArgs e)
        {

            Point point = e.MouseDevice.GetPosition(this);
            double x = point.X;

            if ( x < BorderWidth )
            {
                dragMode = DragMode.ResizeLeft;
                Mouse.OverrideCursor = Cursors.SizeWE;
                PartEditor.DragMode.Content = $"Left B: {BorderWidth} W: {Width} X: {x} maxW: {maxwidth} minW: {minwidth}";
            }
            else if ( x > Width-BorderWidth )
            {
                dragMode = DragMode.ResizeRight;
                Mouse.OverrideCursor = Cursors.SizeWE;
                PartEditor.DragMode.Content = $"Right B: {BorderWidth} W: {Width} X: {x} maxW: {maxwidth} minW: {minwidth}";
            }
            else
            {
                dragMode = DragMode.Move;
                Mouse.OverrideCursor = Cursors.Hand;
                PartEditor.DragMode.Content = $"Body B: {BorderWidth} W: {Width} X: {x} maxW: {maxwidth} minW: {minwidth}";
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
            note.Length = Width / PartEditor.xScale;
        }

        public void SetText(string _l)
        {
            Text = _l;
            note.Lyric = Text;
        }

        private void NoteThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double deltaHorizontal;
            double width;
            PartEditor.DragModeAdd.Content = $"hc: {e.HorizontalChange}";

            if (dragMode == DragMode.Move)
            {            
                deltaHorizontal = Math.Min(e.HorizontalChange, ActualWidth - MinWidth);
                double deltaVertical = Math.Min(e.VerticalChange, ActualWidth - MinWidth);
                Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
                Canvas.SetTop(this, Canvas.GetTop(this) + deltaVertical);
            }
            else if(dragMode == DragMode.ResizeLeft)
            {
                deltaHorizontal = e.HorizontalChange;
                width = Width - deltaHorizontal;
                if (width > maxwidth) width = maxwidth;
                if (width < minwidth) width = minwidth;
                else Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
                Width = width;
            }
            else if (dragMode == DragMode.ResizeRight)
            {
                deltaHorizontal = e.HorizontalChange;
                width = WidthInit + deltaHorizontal;
                if (width > maxwidth) width = maxwidth;
                if (width < minwidth) width = minwidth;
                Width = width;
                PartEditor.DragModeAdd.Content += $" dh: {deltaHorizontal} w: {width} wInit: {WidthInit}";
            }

            e.Handled = true;
        }
    }
}
