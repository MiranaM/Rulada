using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PianoRoll.Model;

namespace PianoRoll.View
{
    /// <summary>
    ///     Логика взаимодействия для SingerDialog.xaml
    /// </summary>
    public partial class SingerDialog : Window
    {
        private readonly Part part;

        public SingerDialog()
        {
            part = Project.Current.Tracks[0].Parts[0];
            InitializeComponent();
            Name.Content = part.Track.Singer.Name;
            Author.Content = $"Author: {part.Track.Singer.Author}";
            if (part.Track.Singer.Image != null)
            {
                var imagepath = Path.Combine(part.Track.Singer.Dir, part.Track.Singer.Image);
                if (File.Exists(imagepath)) Avatar.Source = new BitmapImage(new Uri(imagepath));
            }

            DrawOto();
        }

        private void DrawOto()
        {
            OtoView.Items.Clear();
            foreach (var phoneme in part.Track.Singer.Phonemes)
            {
                var line = new[]
                {
                    phoneme.Alias, phoneme.File, phoneme.Offset.ToString(), phoneme.Consonant.ToString(),
                    phoneme.Cutoff.ToString(), phoneme.Preutter.ToString(), phoneme.Overlap.ToString()
                };
                var item = new ListViewItem();
                item.Content = line;
                OtoView.Items.Add(phoneme);
            }
        }
    }
}