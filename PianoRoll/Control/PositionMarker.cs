using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using PianoRoll.Model;
using PianoRoll.Themes;
using PianoRoll.Util;

namespace PianoRoll.Control
{
    public class PositionChangedArgs : EventArgs
    {
        public long position;

        public PositionChangedArgs(long _p)
        {
            position = _p;
        }
    }

    public class PositionMarker
    {
        private Polygon Head;
        private Line Line;
        private readonly Canvas Canvas;
        private readonly ScrollViewer ScrollViewer;

        public PositionMarker(Canvas canvas, ScrollViewer scrollViewer)
        {
            Canvas = canvas;
            ScrollViewer = scrollViewer;
            DrawPositionMarker();
        }

        private void DrawPositionMarker()
        {
            Head = new Polygon {Fill = Schemes.Current.positionMarkerHead };
            Head.Points.Add(new Point(10, 0));
            Head.Points.Add(new Point(10, 5));
            Head.Points.Add(new Point(0, 15));
            Head.Points.Add(new Point(-10, 5));
            Head.Points.Add(new Point(-10, 0));
            Line = new Line
            {
                Stroke = Schemes.Current.positionMarkerLine,
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
        ///     Move marker to wave sample position
        /// </summary>
        /// <param name="x"></param>
        public void MoveTo(long sample)
        {
            var ms = sample / 44.1;
            long tick = MusicMath.Current.MillisecondToTick(ms);
            var x = MusicMath.Current.GetNoteXPosition(tick) - PartEditor.ScrollPosition.X;
            MoveTo(x);
        }

        /// <summary>
        ///     Move marker to x position on given canvas
        /// </summary>
        /// <param name="x"></param>
        public void MoveTo(double x)
        {
            Canvas.SetLeft(Head, x);
            Canvas.SetLeft(Line, x);
        }

        public async void MoveAsync2()
        {
            for (long i = 0; i < Render.Current.PlayerLength; i += 100)
            {
                await Task.Delay(100);
                var ms = i / 44.1;
                long tick = MusicMath.Current.MillisecondToTick(ms);
                var x = MusicMath.Current.GetNoteXPosition(tick) - PartEditor.ScrollPosition.X;
                MoveTo(Render.Current.PlayerPosition);
            }
        }

        public async void MoveAsync()
        {
            // bool result = await Move();
        }

        private Task<bool> Move()
        {
            return Task.Run(() =>
            {
                while (Render.Current.PlayerPosition < Render.Current.PlayerLength)
                {
                    Thread.Sleep(100);
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        // MoveTo(Render.PlayerPosition);
                        PartEditor.Instance.Debug1.Content = Render.Current.PlayerPosition.ToString();
                    });
                }

                return true;
            });
        }
    }
}