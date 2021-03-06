﻿using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
        public SingerManager SingerManager;
        public Project Project;

        public MainWindow()
        {

            InitializeComponent();
            NoteLength.ItemsSource = Settings.Current.NotesLengths.Keys;
            NoteLength.SelectedIndex = 2;
            SingerManager = new SingerManager();
            New();
        }

        private void SetProject()
        {
            Playlist.Init(Project);
        }

        private void SetPosition()
        {
            PartEditor.scrollViewer.ScrollToVerticalOffset(Settings.Current.LastV);
            PartEditor.scrollViewer.ScrollToHorizontalOffset(Settings.Current.LastV);
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
            Project = new Project(SingerManager.DefaultSinger);
            Project.Dir = dir;
            Project.IsNew = false;

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
            Project.AddTrack(track);
            PartEditor.DrawNotes();
            InitElements();
        }

        private void ImportUst(string dir)
        {
            Clear();
            Project = new Project(SingerManager.DefaultSinger);
            SetProject();
            var track = new Track(Project.DefaultSinger);
            var part = track.AddPart();

            Ust.Current.Import(part, dir);
            PartEditor.Clear();
            PartEditor.Part = part;
            Settings.Current.LastFile = dir;
            ImportTrack(track);
            SetPosition();
        }

        private void New()
        {
            TransitionTool.Current.Load(Settings.Current.TransitionTool);
            Project = new Project(SingerManager.DefaultSinger);
            SetProject();
            var track = Project.AddTrack();
            var part = track.AddPart();
            Playlist.AddTrack(track);
            PartEditor.Part = part;
            PartEditor.DrawPart();
            PartEditor.DrawNotes();
            InitElements();
            SetPosition();
        }

        private void InitElements()
        {
            Tempo.Content = $"{Settings.Current.Tempo.ToString("f2")} BPM";
            BeatInfo.Content = $"{Settings.Current.BeatPerBar}/4, 1/{Settings.Current.BeatPerBar * Settings.Current.BeatUnit}";
            PartEditor.Resize();
        }

        private void Save(string dir)
        {
            var textWritter = new XmlTextWriter(dir, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("srnx");
            textWritter.WriteEndElement();
            textWritter.Close();
            Project.Dir = dir;
            Project.IsNew = false;
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
            Settings.Current.LastV = PartEditor.scrollViewer.VerticalOffset;
            Settings.Current.LastH = PartEditor.scrollViewer.HorizontalOffset;
            Settings.Current.Save();
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void ShowTempoDialog()
        {
            var dialog = new TempoDialog();
            dialog.ShowDialog();
            Tempo.Content = Settings.Current.Tempo.ToString("f2");
        }

        private void PartEditor_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(PartEditor);
            CursorTrack.Content = $"[{p.X.ToString("F2")} {p.Y.ToString("F2")}]";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Current.Send(PartEditor.Part);
        }

        private void StopPlayButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Current.Stop();
        }

        private void PausePlayButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Current.Pause();
        }

        private void PlayRenderedButton_Click(object sender, RoutedEventArgs e)
        {
            Render.Current.Play();
        }

        private void Tempo_Click(object sender, MouseButtonEventArgs e)
        {
            ShowTempoDialog();
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
            if (Project.IsNew)
            {
                var dir = ShowSaveFileDialog("Sirin Files (*.srnx)|*.srnx|All Files (*.*)|*.*");
                if (dir == "")
                    return;
                Save(dir);
            }
            else
            {
                Save(Project.Dir);
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
            projectDialog.Init(SingerManager, Project);
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

        private void ShowRenderPartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RenderPartView.Visibility == Visibility.Collapsed && PartEditor.Part.RenderPart != null)
            {
                RenderPartView.SetPart(PartEditor.Part.RenderPart);
                RenderPartView.Visibility = Visibility.Visible;
                PartEditor.Visibility = Visibility.Collapsed;
            }
            else
            {
                PartEditor.Visibility = Visibility.Visible;
                RenderPartView.Visibility = Visibility.Collapsed;
            }
        }

        private void NoteLength_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox tempBox = (ComboBox) sender;
            //tempBox.SelectedItem.ToString();
            Settings.Current.CurrentNoteLength = Settings.Current.NotesLengths[tempBox.SelectedItem.ToString()];
        }
    }
}