using PianoRoll.Model;
using PianoRoll.Util;
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
    /// Логика взаимодействия для Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {
        public int MaxDivider = 4;
        public bool doSnap = true;
        public int octaves = 7;

        double xScale = 1.0 / 15;
        double yScale = 15;
        long lastPosition;
        int minBars = 4;
        double minWidth = 500;
        double minHeight = 300;
        double lastHeigth = 5;

        public Playlist()
        {
            xScale = (80.0 / Settings.Resolution);
            minWidth = minBars * Project.BeatPerBar * Settings.Resolution;
            InitializeComponent();
            ContentCanvas.Loaded += new RoutedEventHandler(SetMinSizes);
            DrawGrid();
        }

        public void SetMinSizes(object sender, RoutedEventArgs e)
        {
            minWidth = ContentCanvas.ActualWidth;
            minHeight = ContentCanvas.ActualHeight;
        }

        public void AddTrack()
        {
            TrackControll trackControll = new TrackControll(lastHeigth, minWidth);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        public void AddTrack(Track track)
        {
            TrackControll trackControll = new TrackControll(lastHeigth, minWidth, track);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        public void AddSoundTrack()
        {
            SoundTrackControl trackControll = new SoundTrackControl(lastHeigth, minWidth);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        public void AddSoundTrack(SoundTrack soundTrack)
        {
            SoundTrackControl trackControll = new SoundTrackControl(lastHeigth, minWidth, soundTrack);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        void Resize()
        {
            var height = minHeight > lastHeigth ? minHeight : lastHeigth;
            var width = minWidth > lastPosition ? minWidth : lastPosition;
            if (minHeight < lastHeigth) ContentCanvas.Height = lastHeigth;
            if (minHeight < lastHeigth) HeaderCanvas.Height = lastHeigth;
            if (minWidth < lastPosition) ContentCanvas.Width = lastPosition;
        }

        public void Clear()
        {
            lastPosition = 0;
            lastHeigth = 0;
            ContentCanvas.Children.Clear();
            HeaderCanvas.Children.Clear();
            GridCanvas.Children.Clear();
            DrawGrid();
        }

        private void DrawGrid()
        {
            double width = lastPosition > minWidth ? lastPosition : minWidth;
            GridCanvas.Children.Clear();
            int beat = 0;
            for (long n = 0; n < width; n += Settings.Resolution)
            {
                Line line = new Line();
                line.X1 = n * xScale + 0.5;
                line.X2 = n * xScale + 0.5;
                line.Y1 = 0;
                line.Y2 = 20;
                if (beat % 4 == 0)
                {
                    line.Stroke = Themes.foreBrush;
                }
                else
                {
                    line.StrokeDashCap = PenLineCap.Flat;
                    line.Stroke = Themes.foreBrush;
                    line.Y1 = 7;
                }
                GridCanvas.Children.Add(line);
                beat++;
            }
        }

        private void AddSoundTrack_Click(object sender, RoutedEventArgs e)
        {
            AddSoundTrack();
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            AddTrack();
        }
    }
}
