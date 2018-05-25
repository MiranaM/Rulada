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
using System.IO;

namespace PianoRoll.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }


        private void MenuItemOpenUst_Click(object sender, RoutedEventArgs e)
        {
            LoadUST();
            InitElementsAfterUSTLoading();
        }

        private void InitElementsAfterUSTLoading()
        {
            SingerName.Content = USinger.Name;
            SingerName.IsEnabled = true;
            VoiceMenu.IsEnabled = true;
            RenderMenu.IsEnabled = true;

        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {            
            Window1 settings = new Window1();            
            if(settings.ShowDialog().Value == true)
            {
                USinger.Load(Ust.uSettings["VoiceDir"]);                
                SingerName.Content = System.IO.Path.GetFileName(Ust.uSettings["VoiceDir"]);
            }
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
                Ust.BuildPitch();
                this.PianoRollControl.UNotes = Ust.NotesList;                
            }
        }

        private void Save()
        {

        }

        private void Exit()
        {
            this.Close();
        }

        private void ShowSingerDialog()
        {
            SingerDialog dialog = new SingerDialog();
            dialog.Show();
        }

        private void PianoRollControl_MouseMove(object sender, MouseEventArgs e)
        {
            this.CursorTrack.Content = e.GetPosition(this.PianoRollControl);
        }

        private void SingerNameInfo_dClick(object sender, RoutedEventArgs e)
        {
            ShowSingerDialog();
        }

        private void SingerName_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowSingerDialog();
        }

        private void SingerName_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
