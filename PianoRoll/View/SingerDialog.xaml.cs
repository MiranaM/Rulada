using System;
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
        public SingerDialog()
        {
            InitializeComponent();
            Name.Content = USinger.Name;
            Author.Content = $"Author: {USinger.Author}";
            //string imagepath = System.IO.Path.Combine(USinger.UPath, USinger.Image);
            string imagepath = System.IO.Path.Combine(Environment.CurrentDirectory, ".\\Image\\avatar.png");
            
            Avatar.Source = new BitmapImage(new Uri(imagepath));
            DrawOto();
        }

        void DrawOto()
        {
            OtoView.Items.Clear();
            foreach (UOto oto in USinger.Otos)
            {
                string[] line = new string[]
                {
                    oto.Alias,
                    oto.File,
                    oto.Offset.ToString(),
                    oto.Consonant.ToString(),
                    oto.Cutoff.ToString(),
                    oto.Preutter.ToString(),
                    oto.Overlap.ToString()
                };
                ListViewItem item = new ListViewItem();
                item.Content = line;
                OtoView.Items.Add(oto);
            }
        }
    }
}
