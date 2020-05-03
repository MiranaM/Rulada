using System.Windows.Controls;
using System.Windows.Input;
using PianoRoll.View;

namespace PianoRoll.Control
{
    /// <summary>
    ///     Логика взаимодействия для TrackHeader.xaml
    /// </summary>
    public partial class TrackHeader : UserControl
    {
        public TrackHeader()
        {
            InitializeComponent();
        }

        private void ShowSingerDialog()
        {
            var dialog = new SingerDialog();
            dialog.Show();
        }

        private void Singer_Click(object sender, MouseButtonEventArgs e)
        {
            ShowSingerDialog();
        }
    }
}