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
    /// Логика взаимодействия для TempoDialog.xaml
    /// </summary>
    public partial class TempoDialog : Window
    {
        public TempoDialog()
        {
            InitializeComponent();
            NewTempo.Text = Project.Tempo.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(NewTempo.Text, out double tempo))
                Project.Tempo = tempo;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
