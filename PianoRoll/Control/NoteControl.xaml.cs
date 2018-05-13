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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PianoRoll.Control
{
    /// <summary>
    /// Логика взаимодействия для NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl
    {
        public string Text { get; set; }

        public NoteControl()
        {
            InitializeComponent();
        }

        //зафиксить текст _l
        public void SetText (string _l)
        {
            Text = _l;
            this.Lyric.Content = Text;
            this.EditLyric.Text = Text;
        }

        private void Lyric_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.EditLyric.Visibility = Visibility.Visible;
        }

        private void EditLyric_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {                
                SetText(this.EditLyric.Text);
                this.EditLyric.Visibility = Visibility.Hidden;
            }
        }
    }
}
