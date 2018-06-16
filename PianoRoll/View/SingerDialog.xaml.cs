using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PianoRoll.Model;

namespace PianoRoll.View
{
    /// <summary>
    /// Логика взаимодействия для SingerDialog.xaml
    /// </summary>
    public partial class SingerDialog : Window
    {
        Part part;

        public SingerDialog()
        {
            part = Project.Current.Tracks[0].Parts[0];
            InitializeComponent();
            Name.Content = part.Singer.Name;
            Author.Content = $"Author: {part.Singer.Author}";
            if (part.Singer.Image != null)
            {
                string imagepath = System.IO.Path.Combine(part.Singer.Dir, part.Singer.Image);
                if (File.Exists(imagepath)) Avatar.Source = new BitmapImage(new Uri(imagepath));
            }

            DrawOto();
        }

        void DrawOto()
        {
            OtoView.Items.Clear();
            foreach (Phoneme phoneme in part.Singer.Phonemes)
            {
                string[] line = new string[]
                {
                    phoneme.Alias,
                    phoneme.File,
                    phoneme.Offset.ToString(),
                    phoneme.Consonant.ToString(),
                    phoneme.Cutoff.ToString(),
                    phoneme.Preutter.ToString(),
                    phoneme.Overlap.ToString()
                };
                ListViewItem item = new ListViewItem();
                item.Content = line;
                OtoView.Items.Add(phoneme);
            }
        }
    }
}
