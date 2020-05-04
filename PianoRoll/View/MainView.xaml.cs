using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Microsoft.Win32;
using PianoRoll.Model;

namespace PianoRoll.View
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
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

        private void SetPosition()
        {
            PartEditor.scrollViewer.ScrollToVerticalOffset(Settings.LastV);
            PartEditor.scrollViewer.ScrollToHorizontalOffset(Settings.LastV);
        }

        private void Clear()
        {
            PartEditor.Clear();
            Playlist.Clear();
        }

        private void Open(string dir)
        {
            New();
            return;

            Clear();
            var xmlReader = XmlReader.Create(new StringReader(dir));
            File.ReadAllText(dir);
            var Project = new Project();
            Project.Dir = dir;
            Project.Current.IsNew = false;

            // temp
            var track = Project.AddTrack();
            Playlist.AddTrack(track);
            PartEditor.Part = track.Parts[0];
            // endtemp

            // Settings.LastFile = dir;
            PartEditor.DrawNotes();
            InitElements();
        }

        private void ImportTrack(Track track)
        {
            Clear();
            Playlist.AddTrack(track);
            Project.Current.AddTrack(track);
            PartEditor.DrawNotes();
            InitElements();
        }

        private void ImportUst(string dir, bool IsNewProject = true)
        {
            if (IsNewProject) Project.Current = new Project();
            var part = Ust.Import(dir, out var tempo, out var singerDir);
            Project.Tempo = tempo;
            var track = new Track(singerDir);
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
            var project = new Project();
            var track = project.AddTrack();
            var part = track.AddPart();
            Playlist.AddTrack(track);
            PartEditor.Part = part;
            PartEditor.DrawPart();
            PartEditor.DrawNotes();
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
            var textWritter = new XmlTextWriter(dir, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("srnx");
            textWritter.WriteEndElement();
            textWritter.Close();
            Project.Current.Dir = dir;
            Project.Current.IsNew = false;
        }

        private string ShowOpenFileDialog(string filter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value)
                return openFileDialog.FileName;
            return "";
        }

        private string ShowSaveFileDialog(string filter)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog().Value)
                return saveFileDialog.FileName;
            return null;
        }

        private void Exit()
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            Settings.LastV = PartEditor.scrollViewer.VerticalOffset;
            Settings.LastH = PartEditor.scrollViewer.HorizontalOffset;
            Settings.Save();
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void ShowTempoDialog()
        {
            var dialog = new TempoDialog();
            dialog.ShowDialog();
            Tempo.Content = Project.Tempo.ToString("f2");
        }

        private void PartEditor_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(PartEditor);
            CursorTrack.Content = $"[{p.X.ToString("F2")} {p.Y.ToString("F2")}]";
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
            PartEditor.Part.BuildPitch();
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
            var dir = ShowOpenFileDialog("Sirin Files (*.srnx)|*.srnx|All Files (*.*)|*.*");
            if (dir == "")
                return;
            Open(dir);
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (Project.Current.IsNew)
            {
                var dir = ShowSaveFileDialog("Sirin Files (*.srnx)|*.srnx|All Files (*.*)|*.*");
                if (dir == "")
                    return;
                Save(dir);
            }
            else
            {
                Save(Project.Current.Dir);
            }
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dir = ShowSaveFileDialog("Sirin Files (*.srnx)|*.srnx|All Files (*.*)|*.*");
            if (dir == "")
                return;
            Save(dir);
        }

        private void MenuItemImportUst_Click(object sender, RoutedEventArgs e)
        {
            var dir = ShowOpenFileDialog("UST Files (*.ust)|*.ust|All Files (*.*)|*.*");
            if (dir == "")
                return;
            ImportUst(dir);
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
            var projectDialog = new ProjectDialog();
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