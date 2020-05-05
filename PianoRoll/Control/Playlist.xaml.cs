using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PianoRoll.Model;
using PianoRoll.Themes;

namespace PianoRoll.Control
{
    /// <summary>
    ///     Логика взаимодействия для Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {
        public int MaxDivider = 4;
        public bool doSnap = true;
        public int octaves = 7;

        private readonly double xScale = 1.0 / 15;
        private double yScale = 15;
        private long lastPosition;
        private readonly int minBars = 4;
        private double minWidth = 500;
        private double minHeight = 300;
        private double lastHeigth = 5;

        public Playlist()
        {
            xScale = 80.0 / Settings.RESOLUTION;
            minWidth = minBars * Settings.Current.BeatPerBar * Settings.RESOLUTION;
            InitializeComponent();
            ContentCanvas.Loaded += SetMinSizes;
            DrawGrid();
        }

        public void SetMinSizes(object sender, RoutedEventArgs e)
        {
            minWidth = ContentCanvas.ActualWidth;
            minHeight = ContentCanvas.ActualHeight;
        }

        public void AddTrack()
        {
            var trackControll = new TrackControl(lastHeigth, minWidth);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        public void AddTrack(Track track)
        {
            var trackControll = new TrackControl(lastHeigth, minWidth, track);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        public void AddSoundTrack()
        {
            var trackControll = new SoundTrackControl(lastHeigth, minWidth);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        public void AddSoundTrack(SoundTrack soundTrack)
        {
            var trackControll = new SoundTrackControl(lastHeigth, minWidth, soundTrack);
            lastHeigth += 80 + 5.0;
            HeaderCanvas.Children.Add(trackControll.Header);
            ContentCanvas.Children.Add(trackControll.Content);
            Resize();
        }

        private void Resize()
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
            lastHeigth = 5;
            ContentCanvas.Children.Clear();
            HeaderCanvas.Children.Clear();
            GridCanvas.Children.Clear();
            DrawGrid();
        }

        private void DrawGrid()
        {
            var width = lastPosition > minWidth ? lastPosition : minWidth;
            GridCanvas.Children.Clear();
            var beat = 0;
            for (long n = 0; n < width; n += Settings.RESOLUTION)
            {
                var line = new Line();
                line.X1 = n * xScale + 0.5;
                line.X2 = n * xScale + 0.5;
                line.Y1 = 0;
                line.Y2 = 20;
                if (beat % 4 == 0)
                {
                    line.Stroke = Schemes.Current.foreBrush;
                }
                else
                {
                    line.StrokeDashCap = PenLineCap.Flat;
                    line.Stroke = Schemes.Current.foreBrush;
                    line.Y1 = 7;
                }

                GridCanvas.Children.Add(line);
                beat++;
            }
        }

        private void AddSoundTrack_Click(object sender, RoutedEventArgs e)
        {
            AddSoundTrack(Project.Current.AddSoundTrack());
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            AddTrack(Project.Current.AddTrack());
        }
    }
}