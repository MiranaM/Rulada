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
            Settings.Read();
            Singer.FindSingers();
            if (Settings.LastFile != null) ImportUst(Settings.LastFile);
            else New();
            SetPosition();
        }

        void SetPosition()
        {
            PianoRollControl.scrollViewer.ScrollToVerticalOffset(Settings.LastV);
            PianoRollControl.scrollViewer.ScrollToHorizontalOffset(Settings.LastV);
        }

        private void Open(string path)
        {

        }

        private void ImportUst(string dir)
        {
            Project project = new Project();
            Track track = new Track();
            project.Tracks = new Track[] { track };
            Part part = Ust.Import(dir, out double tempo, out string singerDir);
            track.Parts = new Part[] { part };
            Singer singer = project.AddSinger(singerDir);
            part.Singer = singer;
            singer.Load();
            part.RefreshPhonemes();
            PianoRollControl.Part = part;
            PianoRollControl.Draw();
            Settings.LastFile = dir;
            InitElements();
        }

        private void New()
        {
            InitElements();
        }

        private void Save()
        {

        }

        private void InitElements()
        {
            Singer singer = Project.Current.Tracks[0].Parts[0].Singer;
            SingerName.Content = singer.Name;
            if (singer.IsEnabled)
            {
                SingerName.IsEnabled = true;
            }
            RenderMenu.IsEnabled = true;
            Tempo.Content = $"{Project.Tempo.ToString("f2")} BPM";
            BeatInfo.Content = $"{Project.BeatPerBar}/4, 1/{Project.BeatPerBar * Project.BeatUnit}";
            PianoRollControl.Resize();
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            //SettingsWindow settings = new SettingsWindow();            
            //if(settings.ShowDialog().Value == true)
            //{
            //    Singer.Load(Part.uSettings["VoiceDir"]);                
            //    SingerName.Content = new DirectoryInfo(Part.uSettings["VoiceDir"]).Name;

            //}
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

        private void Exit()
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            Settings.LastV = PianoRollControl.scrollViewer.VerticalOffset;
            Settings.LastH = PianoRollControl.scrollViewer.HorizontalOffset;
            Settings.Save();
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        private void ShowSingerDialog()
        {
            SingerDialog dialog = new SingerDialog();
            dialog.Show();
        }

        void ShowTempoDialog()
        {
            TempoDialog dialog = new TempoDialog();
            dialog.ShowDialog();
            Tempo.Content = Project.Tempo.ToString("f2");
        }

        private void PianoRollControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this.PianoRollControl);
            this.CursorTrack.Content = $"[{p.X.ToString("F2")} {p.Y.ToString("F2")}]";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Send();
            Render.Play();
        }

        private void StopPlayButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Stop();
        }

        private void SingerName_Click(object sender, MouseButtonEventArgs e)
        {
            ShowSingerDialog();
        }

        private void SingerName_MouseEnter(object sender, MouseEventArgs e)
        {
            SingerName.FontWeight = FontWeights.Bold;
        }

        private void SingerName_MouseLeave(object sender, MouseEventArgs e)
        {
            SingerName.FontWeight = FontWeights.Normal;
        }

        private void PausePlayButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Pause();
        }

        private void PlayRenderedButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Play();
        }

        private void Tempo_Click(object sender, MouseButtonEventArgs e)
        {
            ShowTempoDialog();
        }

        private void MenuItemPitchOff_Click(object sender, RoutedEventArgs e)
        {
            PianoRollControl.PitchOff();
        }

        private void MenuItemDrawPitch_Click(object sender, RoutedEventArgs e)
        {
            PianoRollControl.DrawPitch();
        }

        private void MenuItemDrawPartPitch_Click(object sender, RoutedEventArgs e)
        {
            PianoRollControl.DrawPartPitch();
        }

        private void LoadUST()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UST Files (*.ust)|*.ust|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value) ImportUst(openFileDialog.FileName);
        }

        private void MenuItemImportUst_Click(object sender, RoutedEventArgs e)
        {
            LoadUST();
        }

        private void MenuItemImportMidi_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemImportVsq_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemExportUst_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemExportMidi_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemExportVsq_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemImportAudio_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
