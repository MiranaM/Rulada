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
            PartEditor.scrollViewer.ScrollToVerticalOffset(Settings.LastV);
            PartEditor.scrollViewer.ScrollToHorizontalOffset(Settings.LastV);
        }

        private void Open(string path)
        {

        }

        private void ImportUst(string dir)
        {
            Project project = new Project();
            Track track = new Track();
            Part part = Ust.Import(dir, out double tempo, out string singerDir);
            Project.Tempo = tempo;
            part.Track = track;
            project.AddTrack(part);
            Playlist.AddTrack(part.Track);
            Singer singer = project.AddSinger(singerDir);
            part.Singer = singer;
            singer.Load();
            part.RefreshPhonemes();
            PartEditor.Part = part;
            PartEditor.Draw();
            Settings.LastFile = dir;
            InitElements();
        }

        private void New()
        {
            PartEditor.Clear();
            Playlist.Clear();
            Project project = new Project();
            Part part = new Part();
            part.Singer = Singer.Load(Settings.DefaultVoicebank);
            project.AddTrack(part);
            Playlist.AddTrack(part.Track);
            PartEditor.Part = part;
            PartEditor.Draw();
            InitElements();
        }

        private void Save()
        {

        }

        private void InitElements()
        {
            Singer singer = Project.Current.Tracks[0].Parts[0].Singer;
            PartEditor.SingerName.Content = singer.Name;
            if (singer.IsEnabled)
            {
                PartEditor.SingerName.IsEnabled = true;
            }
            Tempo.Content = $"{Project.Tempo.ToString("f2")} BPM";
            BeatInfo.Content = $"{Project.BeatPerBar}/4, 1/{Project.BeatPerBar * Project.BeatUnit}";
            PartEditor.Resize();
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
            Settings.LastV = PartEditor.scrollViewer.VerticalOffset;
            Settings.LastH = PartEditor.scrollViewer.HorizontalOffset;
            Settings.Save();
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        void ShowTempoDialog()
        {
            TempoDialog dialog = new TempoDialog();
            dialog.ShowDialog();
            Tempo.Content = Project.Tempo.ToString("f2");
        }

        private void PartEditor_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this.PartEditor);
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
            PartEditor.PitchOff();
        }

        private void MenuItemDrawPitch_Click(object sender, RoutedEventArgs e)
        {
            PartEditor.DrawPitch();
        }

        private void MenuItemDrawPartPitch_Click(object sender, RoutedEventArgs e)
        {
            PartEditor.DrawPartPitch();
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
            New();
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

        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectDialog projectDialog = new ProjectDialog();
            projectDialog.ShowDialog();
        }

        private void PartButton_Click(object sender, RoutedEventArgs e)
        {
            PartEditor.Visibility = Visibility.Visible;
            Playlist.Visibility = Visibility.Hidden;
        }

        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Playlist.Visibility = Visibility.Visible;
            PartEditor.Visibility = Visibility.Hidden;
        }

        private void MenuItemExportAudio_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
