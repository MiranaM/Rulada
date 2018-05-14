using Microsoft.Win32;
using NAudio.Midi;
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
using PianoRoll.Model;

namespace PianoRoll.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SolidColorBrush blackNoteChannelBrush = new SolidColorBrush(System.Windows.Media.Colors.Black);

        public MainWindow()
        {
            InitializeComponent();
        }

        public void EnableSingerInformation(bool statement)
        {
            SingerName.IsEnabled = statement;
            AboutSinger.IsEnabled = statement;
            ChangeSinger.IsEnabled = statement;

            SingerName.Foreground = blackNoteChannelBrush;
            AboutSinger.Foreground = blackNoteChannelBrush;
            ChangeSinger.Foreground = blackNoteChannelBrush;

            SingerName.Content = USinger.Name;
            
        }


        private void MenuItemOpenUst_Click(object sender, RoutedEventArgs e)
        {
            LoadUST();            
            EnableSingerInformation(true);
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        private void MenuItemSinger_Click(object sender, RoutedEventArgs e)
        {
            ShowSingerDialog();
        }

        private void MenuItemPlay_Click(object sender, RoutedEventArgs e)
        {
            Render.Play();
        }

        private void MenuItemStop_Click(object sender, RoutedEventArgs e)
        {
            Render.Stop();
        }




        private void LoadUST()
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UST Files (*.ust)|*.ust|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
            {
                Ust.Load(openFileDialog.FileName);
                USinger.Load(Ust.uSettings["VoiceDir"]);
                this.PianoRollControl.UNotes = Ust.NotesList;
                
            }

        }

        private void Save()
        {

        }

        private void Exit()
        {

        }

        private void ShowSingerDialog()
        {
            SingerDialog dialog = new SingerDialog();
            dialog.Show();
        }

        private void AboutSinger_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowSingerDialog();
        }

        private void MenuItemExport_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void PianoRollControl_MouseMove(object sender, MouseEventArgs e)
        {
            this.CursorTrack.Content = e.GetPosition(this.PianoRollControl);
        }
    }
}
