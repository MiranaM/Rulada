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
using System.Windows.Shapes;
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
            string imagepath = System.IO.Path.Combine(USinger.UPath, USinger.Image);
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
