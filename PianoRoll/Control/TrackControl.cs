﻿using PianoRoll.Model;
using PianoRoll.Themes;
using PianoRoll.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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
            Header.TrackName.Content = $"Track {Project.Current.Tracks.Count}";
            int i = Project.Current.SingerNames.IndexOf(Track.Singer.Name);
            Header.VoicebankName.Content = $"{i + 1}: {Track.Singer.Name}";
            if (Track.Singer.Image != null)
            {
                string imagepath = System.IO.Path.Combine(Track.Singer.Dir, Track.Singer.Image);
                if (File.Exists(imagepath)) Header.Avatar.Source = new BitmapImage(new Uri(imagepath));
            }
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
                Fill = Schemes.black
            };
            Content.SetValue(Canvas.TopProperty, y);
            Content.SetValue(Canvas.LeftProperty, 5.0);
        }

        void AddParts(List<Part> parts)
        {

        }
    }
}