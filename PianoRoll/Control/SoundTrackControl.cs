using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using PianoRoll.Model;
using PianoRoll.Themes;

namespace PianoRoll.Control
{
    internal class SoundTrackControl
    {
        public SoundTrackHeader Header;
        public Rectangle Content;
        public SoundTrack Track;
        public List<Rectangle> Parts = new List<Rectangle>();
        public bool IsMuted = false;
        public bool IsSolo = false;

        public SoundTrackControl(double y, double w)
        {
            Track = Project.Current.AddSoundTrack();
            CreateHeader(y);
            CreateContent(y, w, Header.Height);
        }

        public SoundTrackControl(double y, double w, SoundTrack track)
        {
            Track = track;
            CreateHeader(y);
            CreateContent(y, w, Header.Height);
            AddParts(Track.Parts);
        }

        private void CreateHeader(double y)
        {
            Header = new SoundTrackHeader();
            Header.SetValue(Canvas.TopProperty, y);
            Header.SetValue(Canvas.LeftProperty, 5.0);
        }

        private void CreateContent(double y, double w, double h)
        {
            Content = new Rectangle {Height = h, Width = w, Fill = Schemes.black};
            Content.SetValue(Canvas.TopProperty, y);
            Content.SetValue(Canvas.LeftProperty, 5.0);
        }

        private void AddParts(List<SoundPart> parts)
        {
        }
    }
}