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
using System.Xml;

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
            // if (Settings.LastFile != null) ImportUst(Settings.LastFile);
            // else New();
            New();
            SetPosition();
        }

        void SetPosition()
        {
            PartEditor.scrollViewer.ScrollToVerticalOffset(Settings.LastV);
            PartEditor.scrollViewer.ScrollToHorizontalOffset(Settings.LastV);
        }

        void Clear()
        {
            PartEditor.Clear();
            Playlist.Clear();
        }

        private void Open(string dir)
        {
            New();
            return;

            Clear();
            XmlReader xmlReader = XmlReader.Create(new StringReader(dir));
            File.ReadAllText(dir);
            Project Project = new Project();
            Project.Dir = dir;
            Project.Current.IsNew = false;

            // temp
            Track track = Project.AddTrack();
            Playlist.AddTrack(track);
            PartEditor.Part = track.Parts[0];
            // endtemp

            // Settings.LastFile = dir;
            PartEditor.Draw();
            InitElements();
        }

        private void ImportTrack(Track track)
        {
            Clear();
            Playlist.AddTrack(track);
            Project.Current.AddTrack(track);
            PartEditor.Draw();
            InitElements();
        }

        private void ImportUst(string dir, bool IsNewProject = true)
        {
            if (IsNewProject) Project.Current = new Project();
            Part part = Ust.Import(dir, out double tempo, out string singerDir);
            Project.Tempo = tempo;
            Track track = new Track(singerDir);
            part.RefreshPhonemes();
            track.AddPart(part);
            PartEditor.Part = part;
            Settings.LastFile = dir;
            ImportTrack(track);
        }

        private void New()
        {
            Clear();
            TransitionTool.Load(Settings.TransitionTool);
            Project project = new Project();
            Track track =  project.AddTrack();
            Part part = track.AddPart();
            Playlist.AddTrack(track);
            PartEditor.Part = part;
            PartEditor.Draw();
            InitElements();
        }

        private void InitElements()
        {
            Tempo.Content = $"{Project.Tempo.ToString("f2")} BPM";
            BeatInfo.Content = $"{Project.BeatPerBar}/4, 1/{Project.BeatPerBar * Project.BeatUnit}";
            PartEditor.Resize();
        }

        private void Save(string dir)
        {
            XmlTextWriter textWritter = new XmlTextWriter(dir, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("srnx");
            textWritter.WriteEndElement();
            textWritter.Close();
            Project.Current.Dir = dir;
            Project.Current.IsNew = false;
        }

        private string ShowOpenFileDialog(string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value) return openFileDialog.FileName;
            else return "";
        }

        private string ShowSaveFileDialog(string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog().Value) return saveFileDialog.FileName;
            else return null;
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
            Render.Send(PartEditor.Part);
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

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            string dir = ShowOpenFileDialog("Sirin Files (*.srnx)|*.srnx|All Files (*.*)|*.*");
            if (dir == "") return;
            else Open(dir);
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (Project.Current.IsNew)
            {
                string dir = ShowSaveFileDialog("Sirin Files (*.srnx)|*.srnx|All Files (*.*)|*.*");
                if (dir == "") return;
                else Save(dir);
            }
            else Save(Project.Current.Dir);
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            string dir = ShowSaveFileDialog("Sirin Files (*.srnx)|*.srnx|All Files (*.*)|*.*");
            if (dir == "") return;
            else Save(dir);
        }

        private void MenuItemImportUst_Click(object sender, RoutedEventArgs e)
        {
            string dir = ShowOpenFileDialog("UST Files (*.ust)|*.ust|All Files (*.*)|*.*");
            if (dir == "") return;
            else ImportUst(dir);
        }

        private void MenuItemImportMidi_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemImportVsq_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemImportAudio_Click(object sender, RoutedEventArgs e)
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
