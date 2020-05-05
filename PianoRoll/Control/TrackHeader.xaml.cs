using System.Windows.Controls;
using System.Windows.Input;
using PianoRoll.Model;
using PianoRoll.View;

namespace PianoRoll.Control
{
    /// <summary>
    ///     Логика взаимодействия для TrackHeader.xaml
    /// </summary>
    public partial class TrackHeader : UserControl
    {

        public TrackHeader(Singer singer)
        {
            this.singer = singer;
            InitializeComponent();
        }

        private Singer singer;

        private void ShowSingerDialog()
        {
            var dialog = new SingerDialog(singer);
            dialog.Show();
        }

        private void Singer_Click(object sender, MouseButtonEventArgs e)
        {
            ShowSingerDialog();
        }
    }
}