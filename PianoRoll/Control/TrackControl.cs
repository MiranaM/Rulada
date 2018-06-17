using PianoRoll.Model;
using PianoRoll.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PianoRoll.Control
{
    public class TrackControll
    {
        public TrackHeader Header;
        public Rectangle Content;
        public Track Track;
        public Canvas[] Parts;
        public bool IsMuted = false;
        public bool IsSolo = false;

        public TrackControll(double y, double w)
        {
            Track = Project.Current.AddTrack();
            CreateHeader(y);
            CreateContent(y, w, Header.Height);
        }

        public TrackControll(double y, double w, Track track)
        {
            Track = track;
            CreateHeader(y);
            CreateContent(y, w, Header.Height);
            AddParts(Track.Parts);
        }

        void CreateHeader(double y)
        {
            Header = new TrackHeader();
            Header.SetValue(Canvas.TopProperty, y);
            Header.SetValue(Canvas.LeftProperty, 5.0);
        }

        void CreateContent(double y, double w, double h)
        {
            Content = new Rectangle()
            {
                Height = h,
                Width = w,
                Fill = Themes.black
            };
            Content.SetValue(Canvas.TopProperty, y);
            Content.SetValue(Canvas.LeftProperty, 5.0);
        }

        void AddParts(List<Part> parts)
        {

        }
    }
}
