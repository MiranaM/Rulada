using PianoRoll.Model;
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
        public delegate void RedrawUst();

        public event RedrawUst onUstChanged;

        public string Text { get; set; }

        //public ref UNoteRef;
        
        public NoteControl()
        {
            InitializeComponent();
        }

        public UNote note;


        //зафиксить текст _l
        public void SetText (string _l)
        {
            Text = _l;
            this.Lyric.Content = Text;
            this.EditLyric.Text = Text;
            note.Lyric = Text;          
        }

        private void DrawUstAgain(object sender)
        {

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
                onUstChanged();
            }
        }

        private void Lyric_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Ust.NotesList.Remove(note);
                onUstChanged();
            }

        }

        private void ResizeArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ResizeArea.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            ResizeArea.Visibility = Visibility.Hidden;
        }

    }
}
