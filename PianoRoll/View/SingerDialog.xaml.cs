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
        private readonly Singer singer;

        public SingerDialog(Singer singer)
        {
            this.singer = singer;
            InitializeComponent();
            Name.Content = singer.Name;
            Author.Content = $"Author: {singer.Author}";
            if (singer.Image != null)
            {
                var imagepath = Path.Combine(singer.Dir, singer.Image);
                if (File.Exists(imagepath)) Avatar.Source = new BitmapImage(new Uri(imagepath));
            }

            DrawOto();
        }

        private void DrawOto()
        {
            OtoView.Items.Clear();
            foreach (var phoneme in singer.Otos)
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