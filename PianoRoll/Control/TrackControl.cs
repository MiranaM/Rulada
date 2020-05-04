using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PianoRoll.Model;
using PianoRoll.Themes;
using Path = System.IO.Path;

namespace PianoRoll.Control
{
    public class TrackControl
    {
        public TrackHeader Header;
        public Rectangle Content;
        public Track Track;
        public Canvas[] Parts;
        public bool IsMuted = false;
        public bool IsSolo = false;

        public TrackControl(double y, double w)
        {
            Track = Project.Current.AddTrack();
            CreateHeader(y);
            CreateContent(y, w, Header.Height);
        }

        public TrackControl(double y, double w, Track track)
        {
            Track = track;
            CreateHeader(y);
            CreateContent(y, w, Header.Height);
            Header.TrackName.Content = $"Track {Project.Current.Tracks.Count}";
            Header.VoicebankName.Content = Track.Singer.Name;
            if (Track.Singer.Image != null)
            {
                var imagepath = Path.Combine(Track.Singer.Dir, Track.Singer.Image);
                if (File.Exists(imagepath))
                    Header.Avatar.Source = new BitmapImage(new Uri(imagepath));
            }

            AddParts(Track.Parts);
        }

        private void CreateHeader(double y)
        {
            Header = new TrackHeader();
            Header.SetValue(Canvas.TopProperty, y);
            Header.SetValue(Canvas.LeftProperty, 5.0);
        }

        private void CreateContent(double y, double w, double h)
        {
            Content = new Rectangle {Height = h, Width = w, Fill = Schemes.Current.black };
            Content.SetValue(Canvas.TopProperty, y);
            Content.SetValue(Canvas.LeftProperty, 5.0);
        }

        private void AddParts(List<Part> parts)
        {
        }
    }
}