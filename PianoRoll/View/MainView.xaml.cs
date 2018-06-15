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
            if (Settings.LastFile != null) Open(Settings.LastFile);
            else New();
            SingerName.Content = USinger.Name;
            SetPosition();
        }

        void SetPosition()
        {
            PianoRollControl.scrollViewer.ScrollToVerticalOffset(Settings.LastV);
            PianoRollControl.scrollViewer.ScrollToHorizontalOffset(Settings.LastV);
        }

        private void Open(string path)
        {
            Ust.Load(path);
            USinger.Load(System.IO.Path.Combine(Settings.VoicebankDirectory, Ust.VoiceDir));
            USinger.NoteOtoRefresh();
            this.PianoRollControl.UNotes = Ust.NotesList;
            Settings.LastFile = path;
            InitElements();
        }

        private void New()
        {
            InitElements();
        }

        private void Save()
        {

        }

        private void MenuItemOpenUst_Click(object sender, RoutedEventArgs e)
        {
            LoadUST();
        }

        private void InitElements()
        {
            if (USinger.isEnabled)
            {
                SingerName.Content = USinger.Name;
                SingerName.IsEnabled = true;
            }
            RenderMenu.IsEnabled = true;
            Tempo.Content = $"{Settings.Tempo.ToString("f2")} BPM";
            BeatInfo.Content = $"{Settings.BeatPerBar}/4, 1/{Settings.BeatPerBar*Settings.BeatUnit}";
            DrawPitch.Header = "_Рисовать питч";
            PianoRollControl.Resize();
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();            
            if(settings.ShowDialog().Value == true)
            {
                USinger.Load(Ust.uSettings["VoiceDir"]);                
                SingerName.Content = new DirectoryInfo(Ust.uSettings["VoiceDir"]).Name;

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

        private void MenuItemDrawPitch_Click(object sender, RoutedEventArgs e)
        {
            bool toDraw = PianoRollControl.DrawPitch();
            if (toDraw) DrawPitch.Header = "_Рисовать питч ✓";
            else DrawPitch.Header = "_Рисовать питч";
        }

        private void LoadUST()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UST Files (*.ust)|*.ust|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog().Value) Open(openFileDialog.FileName);
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
            Tempo.Content = Settings.Tempo.ToString("f2");
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
    }
}
