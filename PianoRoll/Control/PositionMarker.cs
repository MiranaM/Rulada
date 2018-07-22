using PianoRoll.Model;
using PianoRoll.Themes;
using PianoRoll.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PianoRoll.Control
{
    public class PositionChangedArgs : EventArgs
    {
        public long position;
        public PositionChangedArgs(long _p) { position = _p; }
    }

    public class PositionMarker
    {
        Polygon Head;
        Line Line;
        Canvas Canvas;
        ScrollViewer ScrollViewer;

        public PositionMarker(Canvas canvas, ScrollViewer scrollViewer)
        {
            Canvas = canvas;
            ScrollViewer = scrollViewer;
            DrawPositionMarker();
        }

        void DrawPositionMarker()
        {
            Head = new Polygon()
            {
                Fill = Schemes.positionMarkerHead
            };
            Head.Points.Add(new Point(10, 0));
            Head.Points.Add(new Point(10, 5));
            Head.Points.Add(new Point(0, 15));
            Head.Points.Add(new Point(-10, 5));
            Head.Points.Add(new Point(-10, 0));
            Line = new Line()
            {
                Stroke = Schemes.positionMarkerLine,
                X1 = 0,
                X2 = 0,
                Y1 = 0,
                Y2 = ScrollViewer.ViewportHeight
            };
            Canvas.Children.Add(Line);
            Canvas.Children.Add(Head);
            Canvas.SetTop(Head, 0);
            Canvas.SetTop(Line, 0);
            Canvas.SetLeft(Head, 0);
            Canvas.SetLeft(Line, 0);
        }

        /// <summary>
        /// Move marker to wave sample position
        /// </summary>
        /// <param name="x"></param>
        public void MoveTo(long sample)
        {
            double ms = sample / 44.1;
            long tick = MusicMath.MillisecondToTick(ms);
            double x = MusicMath.GetNoteXPosition(tick) - PartEditor.ScrollPosition.X;
            MoveTo(x);
        }

        /// <summary>
        /// Move marker to x position on given canvas
        /// </summary>
        /// <param name="x"></param>
        public void MoveTo(double x)
        {
            Canvas.SetLeft(Head, x);
            Canvas.SetLeft(Line, x);
        }

        public async void MoveAsync2()
        {
            for (long i = 0; i < Render.PlayerLength; i += 100)
            {
                await Task.Delay(100);
                double ms = ((double)i) / 44.1;
                long tick = MusicMath.MillisecondToTick(ms);
                double x = MusicMath.GetNoteXPosition(tick) - PartEditor.ScrollPosition.X;
                MoveTo(Render.PlayerPosition);
            }
        }

        public async void MoveAsync()
        {
            // bool result = await Move();
        }

        Task<bool> Move()
        {
            return Task.Run(() =>
            {
                while (Render.PlayerPosition < Render.PlayerLength)
                {
                    Thread.Sleep(100);
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        // MoveTo(Render.PlayerPosition);
                        PartEditor.Instance.Debug1.Content = Render.PlayerPosition.ToString();
                    });
                }
                return true;
            });
        }
    }
}
